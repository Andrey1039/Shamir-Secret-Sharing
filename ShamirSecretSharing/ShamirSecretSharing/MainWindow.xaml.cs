using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Numerics;
using ShamirSecretSharing.Data;
using System.Windows.Documents;
using System.Collections.Generic;

namespace ShamirSecretSharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // Разбиение секрета на части
        private string CreateSecretParts(string secret, int threshold, int numShares, BigInteger prime)
        {
            BigInteger bigSecret = new BigInteger(Encoding.GetEncoding(1251).GetBytes(secret));
            List<Tuple<int, BigInteger>> shares = ShamirSecretProtocol.ShareSecret(bigSecret, threshold, numShares, prime);

            SecretPartsTB.Document.Blocks.Clear();
            string result = string.Empty;

            foreach (Tuple<int, BigInteger> share in shares)
            {
                result += $"{share.Item1}:{share.Item2}\r";
            }

            return result;
        }

        // Восстановление секрета
        private string RecoverSecret(string[] secretParts, int numShares, BigInteger prime)
        {
            List<Tuple<int, BigInteger>> shares = new List<Tuple<int, BigInteger>>();

            Random rand = new Random();
            int[] randNumbers = Enumerable.Range(0, secretParts.Length)
                .OrderBy(x => rand.Next())
                .Take(numShares)
                .ToArray();

            for (int i = 0; i < numShares; i++)
            {
                string[] inputText = secretParts[randNumbers[i]].Trim().Split(':');

                int num = int.Parse(inputText[0]);
                BigInteger partSecret = BigInteger.Parse(inputText[1]);

                shares.Add(new Tuple<int, BigInteger>(num, partSecret));
            }

            BigInteger recoveredSecret = ShamirSecretProtocol.RecoverSecret(shares, prime, numShares);
            byte[] byteText = recoveredSecret.ToByteArray();
            string secret = Encoding.GetEncoding(1251).GetString(byteText);

            return secret;
        }

        // Работа с протоколом
        private void ShamirShare()
        {
            // threshold <= numShares
            // secret < prime

            string secret = OriginalSecretTB.Text;
            int threshold = int.Parse(ThresholdTB.Text);
            int numShares = int.Parse(NumShareTB.Text);
            BigInteger prime = BigInteger.Parse("1949117474374491121499531154375925322401162097726698633624512766087028063654507242883388729918194413");

            string shares = CreateSecretParts(secret, threshold, numShares, prime);
            SecretPartsTB.AppendText(shares);

            string[] secretParts = new TextRange(
                SecretPartsTB.Document.ContentStart,
                SecretPartsTB.Document.ContentEnd
            ).Text.Replace("\r", " ").Replace("\n", "").Split(" ").Where(x => x != "").ToArray();

            ReceivedSecretTB.Text = RecoverSecret(secretParts, int.Parse(NewNumShareTB.Text), prime);
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            ShamirShare();
        }
    }
}

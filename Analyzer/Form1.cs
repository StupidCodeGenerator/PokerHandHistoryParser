using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HandHistories.Objects.GameDescription;
using HandHistories.Parser.Parsers.Factory;
using System.Diagnostics;
using HandHistories.Parser.Parsers.FastParser.Base;
using HandHistories.Parser.Parsers.Exceptions;
using HandHistories.Parser.Parsers.Base;
using System.Threading;

namespace Analyzer {
    public partial class Form1 : Form {
        IHandHistoryParserFactory factory;
        IHandHistoryParser handParser;

        public Form1() {
            InitializeComponent();
            factory = new HandHistoryParserFactoryImpl();
            handParser = factory.GetFullHandHistoryParser(SiteName.Pacific);
        }

        private void buttonAddFiles_Click(object sender, EventArgs e) {
            new Thread(ParseProcess).Start();
        }

        private void ParseProcess() {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                foreach (string fileName in openFileDialog.FileNames) {
                    textBoxFileName.Text = fileName;
                    try {
                        string text = System.IO.File.ReadAllText(fileName);
                        int parsedHands = 0;
                        HandHistoryParserFastImpl fastParser = handParser as HandHistoryParserFastImpl;
                        var hands = fastParser.SplitUpMultipleHandsToLines(text);
                        foreach (var hand in hands) {
                            var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                            parsedHands++;
                        }
                    } catch (Exception ex) {
                        // DO NOTHING
                    }
                }
            }
        }
    }
}

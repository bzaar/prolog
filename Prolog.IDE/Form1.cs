using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Prolog.Runtime;

namespace Prolog.IDE
{
    public partial class Form1 : Form, IDebugEventSink
    {
        private readonly IEnumerator <IDebugEvent> enumerator;

        public Form1(IEnumerable<IDebugEvent> events)
        {
            enumerator = events.GetEnumerator ();

            InitializeComponent ();

            bool autosizing = false;
            listView1.Resize += delegate 
            { 
                if (!autosizing)
                {
                    autosizing = true;
                    listView1.Columns [0].Width = -2;
                    autosizing = false;
                }
            };
            listView1.Columns [0].Width = -2;

            listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;

            EnableButtons ();

            this.gotoButton.Enabled = false;
        }

        void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtons ();
        }

        private void EnableButtons ()
        {
            stepButton.Enabled = running;
            redoButton.Enabled = running && listView1.SelectedItems.Count == 1;
        }

        private void StepButton_Click(object sender, EventArgs e)
        {
            Step ();
            EnableButtons ();
        }

        bool running = true;

        int step;

        private void Step ()
        {
            if (enumerator.MoveNext ())
            {
                enumerator.Current.Accept (this);

                ++step;

                stepTextBox.TextChanged -= StepTextBox_TextChanged;
                stepTextBox.Text = step.ToString();
                stepTextBox.TextChanged += StepTextBox_TextChanged;
            }
            else
            {
                running = false;
            }
        }

        private void RedoButton_Click (object sender, EventArgs e)
        {
            int i = listView1.SelectedItems [0].Index;

            while (running && listView1.Items.Count > i)
            {
                Step ();
            }

            EnableButtons ();
        }

        void IDebugEventSink.Visit(Solution solution)
        {
            
        }

        void IDebugEventSink.Visit(Enter enter)
        {
            var sb = new StringBuilder ();
            var goal = enter.Node.HeadGoal;
            sb.Append (new string (' ', goal.Level * 4));
            SolutionTreePrinter.Print (goal, sb); 
            listView1.Items.Add (sb.ToString ());
        }

        void IDebugEventSink.Visit(Leave leave)
        {
            listView1.Items.RemoveAt (listView1.Items.Count - 1);
        }

        private void StepTextBox_TextChanged(object sender, EventArgs e)
        {
            this.gotoButton.Enabled = true;
        }

        private void GotoButton_Click(object sender, EventArgs e)
        {
            int stepToGoTo;
            if (int.TryParse (stepTextBox.Text, out stepToGoTo))
            {
                if (stepToGoTo > step)
                {
                    while (stepToGoTo > step && running)
                    {
                        Step ();
                    }
                }
                else
                {
                    MessageBox.Show (@"Can only go forward.");
                }
            }
            else
            {
                MessageBox.Show (@"Enter an integer.");
            }

            this.gotoButton.Enabled = false;
        }
    }
}

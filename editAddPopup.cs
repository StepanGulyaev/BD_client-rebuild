using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseView {
    public partial class editAddPopup : Form {
        private int maxWidth = 12 + 12 + 45 + 12;
        private int maxHeight = 40;
        public bool result { get; set; }
        public Dictionary<string, string> newValues { get; set; } =  new Dictionary<string, string>();

        private void createControls(string labelText, string inputText, Point llocation) {
            Label l = new Label();
            TextBox t = new TextBox();

            l.Text = labelText+": ";
            l.Tag = labelText;
            l.Location = llocation;
            SizeF size = l.CreateGraphics().MeasureString(l.Text, l.Font);
            l.Size = new Size((int)size.Width, (int)size.Height);
            t.Text = inputText;
            t.Location = new Point(l.Location.X + (int)size.Width + 5, l.Location.Y - 3);
            if (maxWidth < t.Location.X + 110) {
                maxWidth = t.Location.X + 110;
            }
            maxHeight += 30;

            this.Controls.Add(l);
            this.Controls.Add(t);
        }
        public editAddPopup(string name, Dictionary<string, string> inputs) {
            InitializeComponent();
            this.Text = name;
            int mult = 0;
            foreach (string key in inputs.Keys) {
                createControls(key, inputs[key], new Point(12, 18 + 30 * mult));
                mult++;
            }
            applyBtn.Location = new Point(12, maxHeight - 30);
            cancelBtn.Location = new Point(12 + 12 + 75, maxHeight - 30);
            this.Width = maxWidth + 20;
            this.Height = maxHeight + 40;
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            this.result = false;
            this.Close();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            string lv = "", tv = "";
            foreach (Control c in this.Controls) {
                if (c is Label) {
                    lv = c.Tag.ToString() ;
                } else if (c is TextBox) {
                    tv = c.Text;
                }
                if (tv != "" && lv != "") {
                    newValues.Add(lv, tv);
                    lv = ""; tv = "";
                }
            }
            this.result = true;
            this.Close();
        }
    }
}

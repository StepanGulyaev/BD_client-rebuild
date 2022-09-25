using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace DatabaseView {
    public partial class editAddPopup : Form {
        private int maxWidth = 12 + 12 + 45 + 12;
        private int maxHeight = 40;
        public bool result { get; set; }
        public Dictionary<string, string> newValues { get; set; } =  new Dictionary<string, string>();

        private void createControls(string labelText, string inputText, Point llocation, bool en) {
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

            if (!en){ 
                t.Enabled = false;
            } 
        }

        private void createForeignKeyComboBox(string labelText, string f_key, Point llocation, NpgsqlConnection con)
            {
            Label l = new Label();
            ComboBox c = new ComboBox();
            c.DropDownStyle = ComboBoxStyle.DropDownList;

            l.Text = labelText + ": ";
            l.Tag = labelText;
            l.Location = llocation;
            SizeF size = l.CreateGraphics().MeasureString(l.Text, l.Font);
            l.Size = new Size((int)size.Width, (int)size.Height);
            c.Location = new Point(l.Location.X + (int)size.Width + 5, l.Location.Y - 3);
            if (maxWidth < c.Location.X + 110)
                {
                maxWidth = c.Location.X + 110;
                }
            maxHeight += 30;

            NpgsqlCommand get_ref_table_cmd = new NpgsqlCommand("WITH unnested_confkey AS ( " +
                "SELECT oid, unnest(confkey) as confkey " +
                "FROM pg_constraint), " +
                "unnested_conkey AS( " +
                "SELECT oid, unnest(conkey) as conkey " +
                "FROM pg_constraint) " +
                "select " +
                "referenced_tbl.relname AS referenced_table " +
                "FROM pg_constraint c " +
                "LEFT JOIN unnested_conkey con ON c.oid = con.oid " +
                "LEFT JOIN pg_class tbl ON tbl.oid = c.conrelid " +
                "LEFT JOIN pg_attribute col ON(col.attrelid = tbl.oid AND col.attnum = con.conkey) " +
                "LEFT JOIN pg_class referenced_tbl ON c.confrelid = referenced_tbl.oid " +
                "LEFT JOIN unnested_confkey conf ON c.oid = conf.oid " +
                "LEFT JOIN pg_attribute referenced_field ON(referenced_field.attrelid = c.confrelid AND referenced_field.attnum = conf.confkey) " +
                $"WHERE c.contype = 'f' AND col.attname = '{f_key}';", con);
            string ref_table = get_ref_table_cmd.ExecuteScalar().ToString();

            NpgsqlCommand pk_cmd = new NpgsqlCommand("select kcu.column_name as key_column " +
            " from information_schema.table_constraints tco" +
            " join information_schema.key_column_usage kcu " +
            "      on kcu.constraint_name = tco.constraint_name" +
            "      and kcu.constraint_schema = tco.constraint_schema" +
            "      and kcu.constraint_name = tco.constraint_name" +
            $" where tco.constraint_type = 'PRIMARY KEY' and kcu.table_name='{ref_table}';", con);
            string primary_key_ref_table = pk_cmd.ExecuteScalar().ToString();

            NpgsqlCommand get_all_pk_cmd = new NpgsqlCommand($"SELECT {primary_key_ref_table} FROM {ref_table};",con);
            NpgsqlDataReader reader = get_all_pk_cmd.ExecuteReader();
            c.Items.Clear();
            if (reader.HasRows){
                while (reader.Read()){
                    c.Items.Add(reader.GetInt32(0));
                    }
                }
            reader.Close();

            this.Controls.Add(l);
            this.Controls.Add(c);
            }

        public editAddPopup(string name, Dictionary<string, string> inputs, string current_table, NpgsqlConnection con) {
            InitializeComponent();
            this.Text = name;
            int mult = 0;
            NpgsqlCommand pk_cmd = new NpgsqlCommand("select kcu.column_name as key_column " +
            " from information_schema.table_constraints tco" +
            " join information_schema.key_column_usage kcu " +
            "      on kcu.constraint_name = tco.constraint_name" +
            "      and kcu.constraint_schema = tco.constraint_schema" +
            "      and kcu.constraint_name = tco.constraint_name" +
            $" where tco.constraint_type = 'PRIMARY KEY' and kcu.table_name='{current_table}';", con);
            string primary_key = pk_cmd.ExecuteScalar().ToString();

            NpgsqlCommand fk_cmd = new NpgsqlCommand("select kcu.column_name as key_column " +
           " from information_schema.table_constraints tco" +
           " join information_schema.key_column_usage kcu " +
           "      on kcu.constraint_name = tco.constraint_name" +
           "      and kcu.constraint_schema = tco.constraint_schema" +
           "      and kcu.constraint_name = tco.constraint_name" +
           $" where tco.constraint_type = 'FOREIGN KEY' and kcu.table_name='{current_table}';", con);
            NpgsqlDataReader reader = fk_cmd.ExecuteReader();
            List<string> foreign_keys = new List<string>();

            if (reader.HasRows) {
                while (reader.Read()) {
                    foreign_keys.Add(reader.GetString(0)); 
                }
            }
            reader.Close();

            foreach (string key in inputs.Keys) {
                if (key == primary_key) {
                    NpgsqlCommand new_id_command = null;
                    if (name == "Add") {
                        new_id_command = new NpgsqlCommand($"SELECT MAX({primary_key})+1 FROM {current_table};", con);
                        string new_id = new_id_command.ExecuteScalar().ToString();
                        createControls(key, new_id, new Point(12, 18 + 30 * mult), false);
                    }
                    else if (name == "Edit") {
                        createControls(key, inputs[key], new Point(12, 18 + 30 * mult),false);
                    }
                
                } 
                else if (foreign_keys.Contains(key)) {
                    createForeignKeyComboBox(key, key, new Point(12, 18 + 30 * mult),con);
                    } else {  
                    createControls(key, inputs[key], new Point(12, 18 + 30 * mult), true);
                }




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

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
    public partial class main : Form {
        private static string current_table;
        private static bool editingRights = true;
        private static NpgsqlConnection con;
        private static DataSet ds = new DataSet();
        private static Dictionary<string, string> requests = new Dictionary<string, string>();
        private static List<string> all_keys = new List<string> { };
        public void connect(string login, string password) {
            string connection_string = $"Server=localhost;Port=5432;User ID={login};Database=land_db;Password={password};";
            con = new NpgsqlConnection(string.Format(connection_string, login, password));
            if(login.ToLower() == "sania")
                {
                MessageBox.Show("Этот пользователь имеет только право на чтение", "Ок", MessageBoxButtons.OK, MessageBoxIcon.Information);
                editingRights = false;
                }
            con.Open();
        }
        public string capitalize(string str) {
            if (str.Length == 0)
                return "";
            else if (str.Length == 1)
                return char.ToUpper(str[0]) + "";
            else
                return char.ToUpper(str[0]) + str.Substring(1);

        }
        public void createButton(string text, string tag, Control parent, Point location, Size size, EventHandler onclick) {
            Button btn = new Button();
            this.Controls.Add(btn);
            btn.Parent = parent;
            btn.Tag = tag;
            btn.Text = text;
            btn.Size = size;
            btn.Location = location;
            btn.Click += onclick;
        }
        public void showQuery(string query) {
            NpgsqlCommand cmd = new NpgsqlCommand(query, con);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            ds.Reset();
            da.Fill(ds);
            dataGridView1.DataSource = ds.Tables[0];
        }
        public main(string login, string password) {
            InitializeComponent();
            connect(login, password);
            requests.Add("ФИО + ЗП", "SELECT " +
                              "stud_fio AS people_fio, CASE " +
                              "WHEN payment_kty=1 THEN 5000 " +
                              "WHEN payment_kty=2 THEN 7000 " +
                              "WHEN payment_kty=3 THEN 10000 " +
                              "WHEN payment_kty=4 THEN 12000 " +
                              "WHEN payment_kty=5 THEN 15000 " +
                              "ELSE 0 END AS stud_polezen " +
                              "FROM Student AS St " +
                              "INNER JOIN Payment AS Pay ON Pay.payment_stud = st.stud_id " +
                              "ORDER BY payment_kty DESC;");
            requests.Add("Count", "SELECT * FROM stud_spec_quant ORDER BY quantity;");
            requests.Add("Without Mosco", "SELECT brig_id, brig_object, " +
                              "(SELECT stud_id FROM Student " +
                              "WHERE stud_id = brig_stud), " +
                              "(SELECT spec_sprav_id FROM Speciality " +
                              "WHERE spec_id = brig_spec) " +
                              "FROM (SELECT *FROM Brigade " +
                              "WHERE brig_spec != 0) AS stud_special " +
                              "WHERE brig_object != 'Москва'; ");
            requests.Add("Count > 1", "SELECT stud_fio, count(spec_sprav_id) AS quantity " +
                              "FROM Student, Speciality " +
                              "WHERE stud_id = spec_stud " +
                              "GROUP BY stud_fio " +
                              "HAVING count(spec_sprav_id)>1;");
            requests.Add("City + plot", "SELECT " +
                              "brig_object, brig_plot " +
                              "FROM Brigade " +
                              "WHERE brig_stud = ANY " +
                              "(SELECT stud_id FROM Student WHERE stud_university = 'MIREA');");
        }

        private void main_Load(object sender, EventArgs e) {
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' and table_type='BASE TABLE'", con);
            DataSet lds = new DataSet();
            NpgsqlDataAdapter lda = new NpgsqlDataAdapter(cmd);
            lds.Reset();
            lda.Fill(lds);
            int pos = 6;
            foreach (DataRow it in lds.Tables[0].Rows) {
                string btnText = it.ItemArray[0].ToString();
                int btnWidth = btnText.Length * 10 + 5;
                createButton(capitalize(btnText), btnText, queryBox, new Point(pos, 19), new Size(btnWidth, 23), queryBtn_Click);
                pos += 5 + btnWidth;
            }
            queryBox.Width = pos + 10;
            editBox.Location = new Point(pos + 10 + 18, 12);
            requestBox.Location = new Point(pos + 10 + 18, 17 + editBox.Size.Height);
            this.Width = queryBox.Width + editBox.Width + 40;

            int mult = 0;
            foreach (string title in requests.Keys) {
                createButton(title, title, requestBox, new Point(9, 19 + 29 * mult), new Size(194, 23), requestBtn_Click);
                mult += 1;
            }
        }
        private void fillEditBox(string table) {
            if (!editingRights)
                return;
            try {
                DataSet dataSet = new DataSet();
                NpgsqlCommand pk_cmd = new NpgsqlCommand("select kcu.column_name as key_column " +
                " from information_schema.table_constraints tco" +
                " join information_schema.key_column_usage kcu " +
                "      on kcu.constraint_name = tco.constraint_name" +
                "      and kcu.constraint_schema = tco.constraint_schema" +
                "      and kcu.constraint_name = tco.constraint_name" +
                $" where tco.constraint_type = 'PRIMARY KEY' and kcu.table_name='{table}'", con);
                string primary_key = pk_cmd.ExecuteScalar().ToString();
                keyLabel.Text = primary_key + ":";

                NpgsqlCommand pk_list_cmd = new NpgsqlCommand($"SELECT {primary_key} FROM {table}", con);
                NpgsqlDataAdapter adapt1 = new NpgsqlDataAdapter(pk_list_cmd);
                dataSet.Reset();
                adapt1.Fill(dataSet);
                selectedKey.Items.Clear();
                foreach (DataRow name in dataSet.Tables[0].Rows) {
                    selectedKey.Items.Add(name.ItemArray[0].ToString());
                }
            } catch {
                editingRights = false;
                editBox.Enabled = false;
            }
        }
        private void queryBtn_Click(object sender, EventArgs e) {
            if (editingRights) {
                editBox.Enabled = true;
            }
            string table = ((Button)sender).Tag.ToString();
            current_table = table;
            showQuery($"SELECT * FROM {table}");
            fillEditBox(table.ToLower());
        }
        private void requestBtn_Click(object sender, EventArgs e) {
            editBox.Enabled = false;
            string request = ((Button)sender).Tag.ToString();
            showQuery(requests[request]);
        }

        private void addBtn_Click(object sender, EventArgs e) {
            DataSet dataSet = new DataSet();
            NpgsqlCommand cmd = new NpgsqlCommand($"SELECT * FROM {current_table}", con);
            NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
            dataSet.Reset();
            adapt.Fill(dataSet);

            Dictionary<string, string> d = new Dictionary<string, string>();
            int i = 0;
            foreach (var item in dataSet.Tables[0].Rows[0].ItemArray) {
                d.Add(dataSet.Tables[0].Columns[i++].ColumnName, "");
            }
            editAddPopup popup = new editAddPopup("Add", d);
            popup.ShowDialog();
            if (popup.result) {
                int paramCount = popup.newValues.Keys.Count;
                string insert_query = $"INSERT INTO {current_table} VALUES (";
                foreach (string key in popup.newValues.Keys) {
                    insert_query += $"'{popup.newValues[key]}',";
                }
                insert_query = insert_query.Remove(insert_query.Length - 1) + ");";
                NpgsqlCommand ins = new NpgsqlCommand(insert_query, con);
                int result = 0;
                try {
                    result = ins.ExecuteNonQuery();
                } catch { }
                if (result > 0) {
                    MessageBox.Show("Запись была успешно добавлена", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    MessageBox.Show("Произошла ошибка при попытке добавить запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                showQuery($"SELECT * FROM {current_table}");
                fillEditBox(current_table.ToLower());
            }
        }

        private void editBtn_Click(object sender, EventArgs e) {
            if (selectedKey.SelectedIndex < 0) {
                MessageBox.Show("Сначала выберите, какую запись хотите изменить", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                DataSet dataSet = new DataSet();
                NpgsqlCommand cmd = new NpgsqlCommand($"SELECT * FROM {current_table}", con);
                NpgsqlDataAdapter adapt = new NpgsqlDataAdapter(cmd);
                dataSet.Reset();
                adapt.Fill(dataSet);

                Dictionary<string, string> d = new Dictionary<string, string>();
                int i = 0;
                foreach (var item in dataSet.Tables[0].Rows[selectedKey.SelectedIndex].ItemArray) {
                    d.Add(dataSet.Tables[0].Columns[i++].ColumnName, item.ToString());
                }
                editAddPopup popup = new editAddPopup("Edit", d);
                popup.ShowDialog();
                if (popup.result) {
                    int paramCount = popup.newValues.Keys.Count;
                    string update_query = $"UPDATE {current_table} SET ";
                    foreach (string key in popup.newValues.Keys) {
                        update_query += $" {key} = '{popup.newValues[key]}',";
                    }
                    update_query = update_query.Remove(update_query.Length - 1);
                    update_query += $" WHERE {keyLabel.Text.Remove(keyLabel.Text.Length - 1)}='{selectedKey.SelectedItem}'";
                    NpgsqlCommand upd = new NpgsqlCommand(update_query, con);
                    int result = 0;
                    try {
                        result = upd.ExecuteNonQuery();
                    } catch { }
                    if (result > 0) {
                        MessageBox.Show("Значения были успешно изменены", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } else {
                        MessageBox.Show("Произошла ошибка при попытке изменить значения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    showQuery($"SELECT * FROM {current_table}");
                    fillEditBox(current_table.ToLower());
                }
            }
        }

        private void removeBtn_Click(object sender, EventArgs e) {
            if (selectedKey.SelectedIndex < 0) {
                MessageBox.Show("Сначала выберите, какую запись хотите изменить", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                string remove_query = $"DELETE FROM {current_table} ";
                remove_query += $" WHERE {keyLabel.Text.Remove(keyLabel.Text.Length - 1)}='{selectedKey.SelectedItem}'";
                NpgsqlCommand rem = new NpgsqlCommand(remove_query, con);
                int result = 0;
                try {
                    result = rem.ExecuteNonQuery();
                } catch { }
                if (result > 0) {
                    MessageBox.Show("Запись была успешно удалена", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    MessageBox.Show("Произошла ошибка при попытке удалить запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                showQuery($"SELECT * FROM {current_table}");
                fillEditBox(current_table.ToLower());

            }
        }

    }
}
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
                MessageBox.Show("Этот пользователь имеет право только на чтение", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            requests.Add("Налог на участок","SELECT " +
                              "obj_name AS region, CASE " +
                              "WHEN reg_tax = 'да' THEN 5000 " +
                              "WHEN reg_tax = 'нет' THEN 0000 " +
                              "ELSE 0 END AS tax " +
                              "FROM Object " +
                              "INNER JOIN Region on Region.reg_id = Object.obj_reg;");
            requests.Add("Цена свободных участков", "SELECT * FROM reg_svobodno;");
            requests.Add("Размер дорогих участков на КК", "SELECT reg_id, " +
                              "(SELECT obj_square " +
                              "FROM Object " +
                              "WHERE obj_name LIKE '%КК%'), " +
                              "(SELECT sprav_amountofoccupiedland " +
                              "FROM Spravochnik " +
                              "WHERE sprav_name LIKE '%КК%') " +
                              "FROM " +
                              "(SELECT reg_address,reg_id FROM Region) AS Zemliy " +
                              "WHERE " +
                              "reg_address = ANY((SELECT reg_address FROM region WHERE reg_costmeter > 10000));");
            requests.Add("Максимальная цена участка", "SELECT  max(reg_sum), reg_address " +
                              "FROM Region " +
                              "WHERE reg_costmeter >0 " +
                              "GROUP BY reg_id " +
                              "HAVING max(reg_sum)>1750000.00;");
            requests.Add("Размер свободных участков", "SELECT " +
                              "sprav_name, sprav_amountoffreeland " +
                              "FROM Spravochnik " +
                              "WHERE sprav_id  = ANY " +
                              "(SELECT reg_sprav FROM Region WHERE reg_sum>1750000.00);");
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
            if (table == "logs"){
                editBox.Enabled = false;
                }
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
            editAddPopup popup = new editAddPopup("Add", d,current_table,con);
            popup.ShowDialog();
            if (popup.result) {
                int paramCount = popup.newValues.Keys.Count;
                string insert_query = "SELECT ";
                if(current_table == "owners"){
                    insert_query += "add_owners(";
                }else if(current_table == "region"){
                    insert_query += "add_reg(";
                }else if(current_table== "object"){
                    insert_query += "add_obj(";
                }else if (current_table == "spravochnik"){
                    insert_query += "add_sprav(";
                }

                NpgsqlCommand get_table_datatypes = new NpgsqlCommand("SELECT column_name,data_type FROM " +
                    $"information_schema.columns WHERE table_name = '{current_table}' AND ordinal_position <> 1 ORDER BY ordinal_position;",con);
                NpgsqlDataReader reader = get_table_datatypes.ExecuteReader();
                Dictionary<string, string> t_data_types = new Dictionary<string, string>();

                if (reader.HasRows){
                    while (reader.Read()){
                        t_data_types.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
                reader.Close();

                NpgsqlCommand pk_cmd = new NpgsqlCommand("select kcu.column_name as key_column " +
                    " from information_schema.table_constraints tco" +
                    " join information_schema.key_column_usage kcu " +
                    "      on kcu.constraint_name = tco.constraint_name" +
                    "      and kcu.constraint_schema = tco.constraint_schema" +
                    "      and kcu.constraint_name = tco.constraint_name" +
                    $" where tco.constraint_type = 'PRIMARY KEY' and kcu.table_name='{current_table}'", con);
                string primary_key = pk_cmd.ExecuteScalar().ToString();

                foreach (string column in t_data_types.Keys) {
                    if(t_data_types[column] != primary_key) {
                    string dt = t_data_types[column];
                    if (dt == "text" || dt == "date" || dt == "bool" || dt == "timestamp"){
                        insert_query += $"'{popup.newValues[column]}',";
                    }else{
                        insert_query += $"{popup.newValues[column]},";
                    }
                }
            }
            insert_query = insert_query.Remove(insert_query.Length - 1) + ");";
            NpgsqlCommand ins = new NpgsqlCommand(insert_query, con);
            ins.ExecuteNonQuery();
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
                editAddPopup popup = new editAddPopup("Edit", d, current_table, con);
                popup.ShowDialog();
                if (popup.result) {
                    int paramCount = popup.newValues.Keys.Count;
                    foreach (string key in popup.newValues.Keys) {
                        string update_query = $"CALL update_field('{current_table}','{key}',{selectedKey.SelectedItem},'{popup.newValues[key].Replace(',', '.')}');";
                        NpgsqlCommand upd = new NpgsqlCommand(update_query, con);
                        upd.ExecuteNonQuery();
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
                string remove_query = $"CALL delete_record('{current_table}','{keyLabel.Text.Remove(keyLabel.Text.Length - 1)}',{selectedKey.SelectedItem});";
                NpgsqlCommand rem = new NpgsqlCommand(remove_query, con);
                rem.ExecuteNonQuery();
                showQuery($"SELECT * FROM {current_table}");
                fillEditBox(current_table.ToLower());
            }
        }
    }
}
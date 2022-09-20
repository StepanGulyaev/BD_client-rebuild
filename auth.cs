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
    public partial class auth : Form {
        public auth() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            string connection_string = $"Server=localhost;Port=5432;User ID={login.Text};Database=land_db;Password={password.Text};";
            NpgsqlConnection con = new NpgsqlConnection(string.Format(connection_string, login, password));
            bool valid = false;
            try {
                con.Open();
                con.Close();
                valid = true;
            } catch {
                MessageBox.Show("Не удалось подключиться к базе данных с введенной учетной записью!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (valid) {
                try {
                    main m = new main(login.Text, password.Text);
                    this.Hide();
                    m.ShowDialog();
                    this.Close();
                } catch {
                    MessageBox.Show("Произошла ошибка при попытке взаимодействия с базой данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            this.Close();
        }
    }
}

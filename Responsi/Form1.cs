using Npgsql;
using System.Data;

namespace Responsi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadData();
        }
        private NpgsqlConnection conn;
        string connstring = "Host=localhost;Port=5432;Username=postgres;Password=2024;Database=Responsi";

        public DataTable dt;
        public static NpgsqlCommand cmd;
        private string sql = null;
        private DataGridViewRow r;

        private void Connection(string connstring)
        {
            try
            {
                this.connstring = connstring;
                conn = new NpgsqlConnection(connstring);
                conn.Open();

                MessageBox.Show("Connection to PostgreSQL established successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connectring to PostgreSQL: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connection(connstring);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    r = dataGridView1.Rows[e.RowIndex];

                    if (r != null && r.Cells["nama"].Value != null && r.Cells["id_dep"].Value != null)
                    {
                        txtNama.Text = r.Cells["nama"].Value.ToString();
                        cbDepartemen.SelectedItem = r.Cells["id_dep"].Value.ToString();
                    }
                    else
                    {
                        txtNama.Clear();
                        cbDepartemen.SelectedIndex = -1;
                        MessageBox.Show("Data yang dipilih tidak valid atau kosong", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat memilih data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadData()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();

                    dataGridView1.DataSource = null;
                    string sql = "select * from karyawan, departemen WHERE karyawan.id_dep=departemen.id_dep;";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var dt = new DataTable();
                            dt.Load(reader);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading table: " + e.Message, "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbDepartemen.Text) || cbDepartemen.SelectedIndex == -1 || string.IsNullOrEmpty(txtNama.Text))
            {
                MessageBox.Show("Lengkapi nama dan departemen terlebih dahulu.");
                return;
            }

            string nama = txtNama.Text;
            string id_dep = cbDepartemen.Text;
            string id_karyawan = cbDepartemen.Text + "_" + txtNama.Text;

            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();
                    string sql = "SELECT * FROM insert_karyawan(@in_id_karyawan, @in_nama, @in_id_dep)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@in_id_karyawan", id_karyawan);
                        cmd.Parameters.AddWithValue("@in_nama", nama);
                        cmd.Parameters.AddWithValue("@in_id_dep", id_dep);

                        int statuscode = (int)cmd.ExecuteScalar();

                        if (statuscode == 201)
                        {
                            MessageBox.Show("[201] Berhasil menambah karyawan");
                            LoadData();
                            return;
                        }
                        if (statuscode == 409)
                        {
                            throw new Exception("[409] Gagal menambah karyawan");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                return;
            }

            string nama = txtNama.Text;
            string id_dep = cbDepartemen.Text;

            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();
                    string sql = "SELECT * FROM edit_karyawan(@in_id_karyawan, @in_nama, @in_id_dep)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@in_id_karyawan", r.Cells["id_karyawan"].Value);
                        cmd.Parameters.AddWithValue("@in_nama", nama);
                        cmd.Parameters.AddWithValue("@in_id_dep", id_dep);

                        int statusCode = (int)cmd.ExecuteScalar();

                        if (statusCode == 200)
                        {
                            MessageBox.Show("[200] Edit success", "Success");
                            LoadData();
                            return;
                        }
                        if (statusCode == 404)
                        {
                            throw new Exception("[404] karyawan tidak ditemukan");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                return;
            }

            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();
                    string sql = "SELECT * FROM delete_karyawan(@in_id_karyawan)";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@in_id_karyawan", r.Cells["id_karyawan"].Value);

                        int statusCode = (int)cmd.ExecuteScalar();

                        if (statusCode == 204)
                        {
                            MessageBox.Show("[204] Delete success", "Success");
                            LoadData();
                            return;
                        }
                        if (statusCode == 404)
                        {
                            throw new Exception("[404] Karyawan tidak ditemukan");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

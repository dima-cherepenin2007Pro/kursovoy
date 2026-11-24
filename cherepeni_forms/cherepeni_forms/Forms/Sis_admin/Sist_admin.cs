using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cherepeni_forms.Forms.Sis_admin;

namespace cherepeni_forms.Forms
{
    public partial class Sistem_admin : Form
    {
        private int userID;
        private string firstName;
        private string lastName;
        private string roleName;

        public Sistem_admin(
            int userID,
            string firstName,
            string lastName,
            string roleName)
        {
            InitializeComponent();

            this.userID = userID;
            this.firstName = firstName;
            this.lastName = lastName;
            this.roleName = roleName;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form user_view = new Users_view();
            user_view.ShowDialog(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form role_view = new Roles_view();
            role_view.ShowDialog(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form status_view = new Statuses();
            status_view.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form special = new Special();
            special.ShowDialog(this);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

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

namespace cherepeni_forms.Forms.Manager
{
    public partial class Manager_form : Form
    {
        private int userID;
        private string firstName;
        private string lastName;
        private string roleName;
        public Manager_form(
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

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form product_view = new Products_view();
            product_view.ShowDialog(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form category_view = new Category_view();
            category_view.ShowDialog(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

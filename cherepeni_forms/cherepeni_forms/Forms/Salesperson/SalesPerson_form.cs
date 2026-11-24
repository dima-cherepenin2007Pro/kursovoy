using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cherepeni_forms.Forms.Manager;

namespace cherepeni_forms.Forms.Salesperson
{
    public partial class SalesPerson_form : Form
    {
        private int userID;
        private string firstName;
        private string lastName;
        private string roleName;
        public SalesPerson_form(
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

        private void button1_Click(object sender, EventArgs e)
        {
            Form order_maker = new OrderMaker(userID);
            order_maker.ShowDialog(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form order_maker = new Orders_view();
            order_maker.ShowDialog(this);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

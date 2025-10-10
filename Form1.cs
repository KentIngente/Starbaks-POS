using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starbaks_POS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string productName;
        decimal productPrice;
        int productQty;

        decimal amountPaid;
        decimal change;
        decimal total;

        private Dictionary<string, (int Quantity, decimal Price)> products = new Dictionary<string, (int, decimal)>();
        private void Form1_Load(object sender, EventArgs e)
        {
            foreach(Control control in menuTable.Controls)
            {
               if(control is Panel menuPanel)
                {
                    foreach(Control button in menuPanel.Controls)
                    {

                        if((string)button.Tag == "addToCartBtn")
                        {
                            button.Click += addToCart_Click;
                        }
                    }
                }
            }
        }

        private void addToCart_Click(object sender, EventArgs e)
        {

            Panel menuPanel = (sender as Control).Parent as Panel;

            foreach(Control control in menuPanel.Controls)
            {
                if(control is Label label)
                {
                    if ((string)label.Tag == "productName")
                    {
                        productName = label.Text;
                    }

                    if((string)label.Tag == "productPrice")
                    {
                        string price = label.Text.Replace("₱", "");
                        productPrice = Convert.ToDecimal(price);
                    }
                }

                if(control is NumericUpDown inputQty)
                {
                    productQty = Convert.ToInt32(inputQty.Value);
                }
            }

            if (products.ContainsKey(productName))
            {
                var exist = products[productName];
                products[productName] = (exist.Quantity + productQty, productPrice);
            }
            else
            {
                products.Add(productName, (productQty, productPrice));
            }

            RefreshCartGrid();
            CalculateOrder();
        }

        public void RefreshCartGrid()
        {
            dgvOrderList.Rows.Clear();

            foreach(var item in products)
            {
                dgvOrderList.Rows.Add($"{item.Key}", item.Value.Quantity, (item.Value.Price * item.Value.Quantity));
            }
            dgvOrderList.ClearSelection();
        }

        public void CalculateOrder()
        {
            total = 0.00m;

            foreach (var item in products)
            {
                total += item.Value.Price * item.Value.Quantity;
            }

            lblTotal.Text = total.ToString("N2"); 
        }

        private void clearAllBtn_Click(object sender, EventArgs e)
        {
            amountPaid = 0.00m;
            change = 0.00m;
            inputAmountPaid.Text = "";
            lblChange.Text = "";
            
            products.Clear();
            CalculateOrder();
            RefreshCartGrid();
        }

        private void confirmBtn_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(inputAmountPaid.Text, out amountPaid))
            {
                if (amountPaid >= total)
                {
                    change = amountPaid - total;
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine(" ".PadLeft(50, '='));
                    sb.AppendLine("                 STARBAKS COFFEE");
                    sb.AppendLine(" ".PadLeft(50, '='));
                    sb.AppendLine($" Date: {DateTime.Now:MM/dd/yyyy hh:mm tt}".PadLeft(35));
                    sb.AppendLine(" ".PadLeft(50, '='));

                    sb.AppendLine(string.Format(" {0,-22}{1,6}{2,10}{3,10}", "Item", "Qty", "Price", "Total"));
                    sb.AppendLine(" ".PadLeft(50, '-'));

                    foreach (var item in products)
                    {
                        decimal itemTotal = item.Value.Price * item.Value.Quantity;
                        string itemName = item.Key.Length > 22 ? item.Key.Substring(0, 19) + "..." : item.Key;

                        sb.AppendLine(string.Format(" {0,-22}{1,6}{2,10:N2}{3,10:N2}",
                            itemName,
                            item.Value.Quantity,
                            item.Value.Price,
                            itemTotal));
                    }

                    sb.AppendLine(" ".PadLeft(50, '-'));
                    sb.AppendLine(string.Format(" {0,-32}{1,16:N2}", "TOTAL:", total));
                    sb.AppendLine(string.Format(" {0,-32}{1,16:N2}", "CASH:", amountPaid));
                    sb.AppendLine(string.Format(" {0,-32}{1,16:N2}", "CHANGE:", change));
                    sb.AppendLine(" ".PadLeft(50, '='));
                    sb.AppendLine("            Thank you for choosing us!");
                    sb.AppendLine(" ".PadLeft(50, '='));

                    ReceiptForm receiptForm = new ReceiptForm();
                    receiptForm.LoadReceipt(sb.ToString());
                    receiptForm.ShowDialog();

                    clearAllBtn.PerformClick();
                }
                else
                {
                    MessageBox.Show("Insufficient payment. Please pay at least the total amount.",
                                  "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Invalid amount paid input!",
                              "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void calculateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                decimal total = decimal.Parse(lblTotal.Text);
                decimal amountPaid = decimal.Parse(inputAmountPaid.Text);
                decimal change = 0;

                if (amountPaid >= total)
                {
                    change = amountPaid - total;
                    lblChange.Text = change.ToString("N2"); 
                }
                else
                {
                    MessageBox.Show("Amount paid is not enough to cover the total.", "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblChange.Text = "0.00"; 
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}

using System;
using System.Windows.Forms;

namespace RGR
{
    public partial class MainForm : Form
    {
        private TextBox txtDisplay;
        private BigNumber memory = null;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Big Number Calculator";
            this.Size = new System.Drawing.Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Display
            txtDisplay = new TextBox();
            txtDisplay.Location = new System.Drawing.Point(20, 20);
            txtDisplay.Size = new System.Drawing.Size(340, 30);
            txtDisplay.Font = new System.Drawing.Font("Consolas", 16);
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            this.Controls.Add(txtDisplay);

            // Buttons layout
            string[] buttonLabels = {
                "7", "8", "9", "/", "C", "CE",
                "4", "5", "6", "*", "(", ")",
                "1", "2", "3", "-", "sin", "cos",
                "0", ".", "=", "+", "tan", "⌫"
            };

            int startX = 20, startY = 70;
            int btnWidth = 70, btnHeight = 50;
            int spacing = 5;
            int cols = 6;

            for (int i = 0; i < buttonLabels.Length; i++)
            {
                int row = i / cols;
                int col = i % cols;
                Button btn = new Button();
                btn.Text = buttonLabels[i];
                btn.Location = new System.Drawing.Point(startX + col * (btnWidth + spacing),
                                                        startY + row * (btnHeight + spacing));
                btn.Size = new System.Drawing.Size(btnWidth, btnHeight);
                btn.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                btn.Click += Button_Click;
                this.Controls.Add(btn);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string text = btn.Text;

            switch (text)
            {
                case "=":
                    CalculateResult();
                    break;
                case "C":
                case "CE":
                    txtDisplay.Clear();
                    break;
                case "⌫":
                    if (txtDisplay.Text.Length > 0)
                        txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
                    break;
                default:
                    txtDisplay.Text += text;
                    break;
            }
        }

        private void CalculateResult()
        {
            try
            {
                var result = ExpressionParser.Evaluate(txtDisplay.Text);
                txtDisplay.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка обчислення",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDisplay.Text = "";
            }
        }
    }
}
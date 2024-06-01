using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CalculatorApp
{
    public partial class CalculatorForm : Form
    {
        private bool isTypingNumber = false;
        private double result = 0;
        private string operatorUsed = "";

        private Label lblLastCalculation1;
        private Label lblTimestamp;

        private TextBox txtDisplay;

        private SqlConnection connection;

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblLastCalculation1 = new System.Windows.Forms.Label();
            this.lblLastCalculation1.AutoSize = true;
            this.lblLastCalculation1.Location = new System.Drawing.Point(12, 0);
            this.lblLastCalculation1.Name = "lblLastCalculation1";
            this.lblLastCalculation1.Size = new System.Drawing.Size(0, 13);
            this.lblLastCalculation1.TabIndex = 1;

            this.Controls.Add(this.lblLastCalculation1);
            this.txtDisplay = new System.Windows.Forms.TextBox();
            this.SuspendLayout();

            this.txtDisplay.Location = new System.Drawing.Point(12, 20);
            this.txtDisplay.Name = "txtDisplay";
            this.txtDisplay.Size = new System.Drawing.Size(200, 20);
            this.txtDisplay.TabIndex = 0;

            this.Controls.Add(this.txtDisplay);

            Button clearDatabaseButton = new Button();
            clearDatabaseButton.Location = new System.Drawing.Point(220, 50);
            clearDatabaseButton.Name = "btnClearDatabase";
            clearDatabaseButton.Size = new System.Drawing.Size(40, 40);
            clearDatabaseButton.TabIndex = 16;
            clearDatabaseButton.Text = "ClearDB";
            clearDatabaseButton.UseVisualStyleBackColor = true;
            clearDatabaseButton.Click += new System.EventHandler(this.btnClearDatabase_Click);

            this.Controls.Add(clearDatabaseButton);

            // Number buttons
            for (int i = 0; i < 10; i++)
            {
                Button numberButton = new Button();
                numberButton.Location = new System.Drawing.Point(12 + (i % 3) * 50, 50 + (i / 3) * 50);
                numberButton.Name = "btnNumber" + i;
                numberButton.Size = new System.Drawing.Size(40, 40);
                numberButton.TabIndex = i;
                numberButton.Text = i.ToString();
                numberButton.UseVisualStyleBackColor = true;
                numberButton.BackColor = Color.LightSlateGray; // Set background color
                numberButton.Click += new System.EventHandler(this.btnNumber_Click);

                this.Controls.Add(numberButton);
            }

            // Operator buttons
            string[] operators = { "+", "-", "*", "/" };
            for (int i = 0; i < operators.Length; i++)
            {
                Button operatorButton = new Button();
                operatorButton.Location = new System.Drawing.Point(162, 50 + i * 50);
                operatorButton.Name = "btnOperator" + i;
                operatorButton.Size = new System.Drawing.Size(50, 40);
                operatorButton.TabIndex = 10 + i;
                operatorButton.Text = operators[i];
                operatorButton.UseVisualStyleBackColor = true;
                operatorButton.BackColor = Color.LightGoldenrodYellow; // Set background color
                operatorButton.Click += new System.EventHandler(this.btnOperator_Click);

                this.Controls.Add(operatorButton);
            }

            // Equals button
            Button equalsButton = new Button();
            equalsButton.Location = new System.Drawing.Point(65, 200);
            equalsButton.Name = "btnEquals";
            equalsButton.Size = new System.Drawing.Size(80, 40);
            equalsButton.TabIndex = 14;
            equalsButton.Text = "=";
            equalsButton.UseVisualStyleBackColor = true;
            equalsButton.BackColor = Color.LightBlue; // Set background color
            equalsButton.Click += new System.EventHandler(this.btnEquals_Click);

            // Add the equals button to the form
            this.Controls.Add(equalsButton);

            // Square root button
            Button sqrtButton = new Button();
            sqrtButton.Location = new System.Drawing.Point(220, 200);
            sqrtButton.Name = "btnSqrt";
            sqrtButton.Size = new System.Drawing.Size(40, 40);
            sqrtButton.TabIndex = 13;
            sqrtButton.Text = "√";
            sqrtButton.UseVisualStyleBackColor = true;
            sqrtButton.Click += new System.EventHandler(this.btnSqrt_Click);

            // Add the square root button to the form
            this.Controls.Add(sqrtButton);

            // Square button
            Button squareButton = new Button();
            squareButton.Location = new System.Drawing.Point(220, 150);
            squareButton.Name = "btnSquare";
            squareButton.Size = new System.Drawing.Size(40, 40);
            squareButton.TabIndex = 15;
            squareButton.Text = "x²";
            squareButton.UseVisualStyleBackColor = true;
            squareButton.Click += new System.EventHandler(this.btnSquare_Click);

            // Add the square button to the form
            this.Controls.Add(squareButton);

            // Create the label for displaying timestamp
            lblTimestamp = new Label();
            lblTimestamp.Location = new System.Drawing.Point(0, 245);
            lblTimestamp.AutoSize = true;
            lblTimestamp.Name = "lblTimestamp";
            lblTimestamp.TabIndex = 17;
            lblTimestamp.Text = "Last Updated: ";
            this.Controls.Add(lblTimestamp);

            this.Name = "CalculatorForm";
            this.Text = "CalculatorForm";
            this.Load += new System.EventHandler(this.CalculatorForm_Load);
            this.ResumeLayout(false);
        }

        private void CalculatorForm_Load(object sender, EventArgs e)
        {
            txtDisplay.Text = "0";

            //SQL Server connection
            string connectionString = "Data Source=SUBHAN\\SQLEXPRESS;Initial Catalog=Calculator;Integrated Security=True;";
            connection = new SqlConnection(connectionString);

            //Open connection
            connection.Open();

            //operations and results tables if they don't exist
            string createOperationsTableQuery = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[operations]') AND type in (N'U')) " +
                "CREATE TABLE [dbo].[operations] ([id] INT IDENTITY(1,1) PRIMARY KEY, [operation] VARCHAR(255), [last_updated] DATETIME DEFAULT GETDATE());";

            string createResultsTableQuery = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id= OBJECT_ID(N'[dbo].[results]') AND type in (N'U')) " +
                "CREATE TABLE [dbo].[results] ([id] INT IDENTITY(1,1) PRIMARY KEY, [result] FLOAT);";

            using (SqlCommand createOperationsTableCommand = new SqlCommand(createOperationsTableQuery, connection))
            {
                createOperationsTableCommand.ExecuteNonQuery();
            }

            using (SqlCommand createResultsTableCommand = new SqlCommand(createResultsTableQuery, connection))
            {
                createResultsTableCommand.ExecuteNonQuery();
            }

            lasttwo();

            // Close connection
            connection.Close();
        }

        private void lasttwo()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            string retrieveCalculationsQuery = "SELECT TOP 2 operation, result FROM operations o JOIN results r ON o.id = r.id ORDER BY o.id DESC;";
            using (SqlCommand command = new SqlCommand(retrieveCalculationsQuery, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string operation1 = reader.GetString(reader.GetOrdinal("operation"));
                        double result1 = reader.GetDouble(reader.GetOrdinal("result"));

                        lblLastCalculation1.Text = $"Last Calculation: {operation1} = {result1}";
                    }

                }
            }

            connection.Close();
        }

        private void btnNumber_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string buttonText = button.Text;

            if (isTypingNumber)
            {
                txtDisplay.Text += buttonText;
            }
            else
            {
                txtDisplay.Text = buttonText;
                isTypingNumber = true;
            }
        }

        private void btnClearDatabase_Click(object sender, EventArgs e)
        {
            ClearDatabase();
        }

        private void btnOperator_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string buttonText = button.Text;

            if (operatorUsed != "")
            {
                Calculate();
            }

            result = double.Parse(txtDisplay.Text);
            operatorUsed = buttonText;
            isTypingNumber = false;
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            Calculate();
            operatorUsed = "";
        }

        private void Calculate()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            double secondNumber = double.Parse(txtDisplay.Text);
            double currentResult = 0;

            switch (operatorUsed)
            {
                case "+":
                    currentResult = result + secondNumber;
                    break;
                case "-":
                    currentResult = result - secondNumber;
                    break;
                case "*":
                    currentResult = result * secondNumber;
                    break;
                case "/":
                    if (secondNumber != 0)
                    {
                        currentResult = result / secondNumber;
                    }
                    else
                    {
                        MessageBox.Show("Cannot divide by zero!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                default:
                    break;
            }

            txtDisplay.Text = currentResult.ToString();
            isTypingNumber = false;

            // Insert into operations table
            string insertOperationQuery = "INSERT INTO operations (operation) VALUES (@operation);";
            using (SqlCommand command = new SqlCommand(insertOperationQuery, connection))
            {
                command.Parameters.AddWithValue("@operation", $"{result} {operatorUsed} {secondNumber}");
                command.ExecuteNonQuery();
            }

            // Insert  into results table
            string insertResultQuery = "INSERT INTO results (result) VALUES (@result);";
            using (SqlCommand command = new SqlCommand(insertResultQuery, connection))
            {
                command.Parameters.AddWithValue("@result", currentResult);
                command.ExecuteNonQuery();
            }

            result = currentResult;
            lasttwo();
            UpdateTimestamp();
            connection.Close();
        }

        private void ClearDatabase()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            string dropOperationsTableQuery = "IF OBJECT_ID(N'dbo.operations', N'U') IS NOT NULL DROP TABLE dbo.operations;";
            string dropResultsTableQuery = "IF OBJECT_ID(N'dbo.results', N'U') IS NOT NULL DROP TABLE dbo.results;";

            using (SqlCommand dropOperationsTableCommand = new SqlCommand(dropOperationsTableQuery, connection))
            {
                dropOperationsTableCommand.ExecuteNonQuery();
            }

            using (SqlCommand dropResultsTableCommand = new SqlCommand(dropResultsTableQuery, connection))
            {
                dropResultsTableCommand.ExecuteNonQuery();
            }

            string createOperationsTableQuery = "CREATE TABLE [dbo].[operations] ([id] INT IDENTITY(1,1) PRIMARY KEY, [operation] VARCHAR(255), [last_updated] DATETIME DEFAULT GETDATE());";
            string createResultsTableQuery = "CREATE TABLE [dbo].[results] ([id] INT IDENTITY(1,1) PRIMARY KEY, [result] FLOAT);";

            using (SqlCommand createOperationsTableCommand = new SqlCommand(createOperationsTableQuery, connection))
            {
                createOperationsTableCommand.ExecuteNonQuery();
            }

            using (SqlCommand createResultsTableCommand = new SqlCommand(createResultsTableQuery, connection))
            {
                createResultsTableCommand.ExecuteNonQuery();
            }

            connection.Close();

            lasttwo();
            UpdateTimestamp();
        }

        private void btnSqrt_Click(object sender, EventArgs e)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            double operand = double.Parse(txtDisplay.Text);
            double sqrtResult = Math.Sqrt(operand);
            txtDisplay.Text = sqrtResult.ToString();
            isTypingNumber = false;

            string insertOperationQuery = "INSERT INTO operations (operation) VALUES (@operation);";
            using (SqlCommand command = new SqlCommand(insertOperationQuery, connection))
            {
                command.Parameters.AddWithValue("@operation", $"√({operand})");
                command.ExecuteNonQuery();
            }

            string insertResultQuery = "INSERT INTO results (result) VALUES (@result);";
            using (SqlCommand command = new SqlCommand(insertResultQuery, connection))
            {
                command.Parameters.AddWithValue("@result", sqrtResult);
                command.ExecuteNonQuery();
            }

            result = sqrtResult;
            lasttwo();
            UpdateTimestamp();
        }

        private void btnSquare_Click(object sender, EventArgs e)
        {

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            double operand = double.Parse(txtDisplay.Text);
            double squareResult = Math.Pow(operand, 2);
            txtDisplay.Text = squareResult.ToString();
            isTypingNumber = false;

            string insertOperationQuery = "INSERT INTO operations (operation) VALUES (@operation);";
            using (SqlCommand command = new SqlCommand(insertOperationQuery, connection))
            {
                command.Parameters.AddWithValue("@operation", $"({operand})²");
                command.ExecuteNonQuery();
            }

            string insertResultQuery = "INSERT INTO results (result) VALUES (@result);";
            using (SqlCommand command = new SqlCommand(insertResultQuery, connection))
            {
                command.Parameters.AddWithValue("@result", squareResult);
                command.ExecuteNonQuery();
            }

            result = squareResult;
            lasttwo();
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            string retrieveTimestampQuery = "SELECT TOP 1 last_updated FROM operations ORDER BY id DESC;";
            using (SqlCommand command = new SqlCommand(retrieveTimestampQuery, connection))
            {
                object lastUpdated = command.ExecuteScalar();
                if (lastUpdated != null && lastUpdated != DBNull.Value)
                {
                    DateTime timestamp = (DateTime)lastUpdated;
                    lblTimestamp.Text = "Last Updated: " + timestamp.ToString();
                }
            }

            connection.Close();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CalculatorForm());
        }
    }
}

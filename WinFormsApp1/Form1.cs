using System.Data;
using System.Text.RegularExpressions;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string inputString = textBox1.Text;
            if (string.IsNullOrWhiteSpace(inputString))
            {
                MessageBox.Show("Please enter some code to analyze.");
                return;
            }

            string comments = @"/\*[\s\S]*?\*/";
            string keywords = @"\b(int|float|string|read|write|repeat|until|if|elseif|else|then|return|endl|main)\b";
            string strings = @"""[^""]*""";
            string numbers = @"\b[0-9]+(\.[0-9]+)?\b";
            string assign = @":=";
            string boolOps = @"&&|\|\|";
            string condOps = @"<>|<=|>=|<|>|=";
            string arithOps = @"[+\-*/]";
            string funcCall = @"\b[a-zA-Z][a-zA-Z0-9]*\s*(?=\()";
            string identifiers = @"\b[a-zA-Z][a-zA-Z0-9]*\b";
            string symbols = @"[;,(){}]";

            string masterPattern =
                $"{comments}|{keywords}|{strings}|{numbers}|{assign}|{boolOps}|" +
                $"{condOps}|{arithOps}|{funcCall}|{identifiers}|{symbols}";

            DataTable dt = new DataTable();
            dt.Columns.Add("Lexeme");
            dt.Columns.Add("Token Type");

            MatchCollection matches = Regex.Matches(inputString, masterPattern);

            foreach (Match m in matches)
            {
                string lex = m.Value.Trim();
                string type = "";

                if (Regex.IsMatch(lex, $"^(?:{comments})$"))
                    type = "COMMENT";
                else if (lex == "int" || lex == "float" || lex == "string")
                    type = "DATATYPE";
                else if (Regex.IsMatch(lex, $"^(?:{keywords})$"))
                    type = "KEYWORD";
                else if (Regex.IsMatch(lex, $"^(?:{strings})$"))
                    type = "STRING";
                else if (Regex.IsMatch(lex, $"^(?:{numbers})$"))
                    type = "NUM";
                else if (lex == ":=")
                    type = "ASSIGN";
                else if (Regex.IsMatch(lex, $"^(?:{boolOps})$"))
                    type = "OP_BOOL";
                else if (Regex.IsMatch(lex, $"^(?:{condOps})$"))
                    type = "OP_COND";
                else if (Regex.IsMatch(lex, $"^(?:{arithOps})$"))
                    type = "OP_ARITH";
                else if (Regex.IsMatch(lex, $"^(?:{funcCall})$"))
                    type = "FUNC_CALL";
                else if (lex == ";") type = "SEMICOLON";
                else if (lex == ",") type = "COMMA";
                else if (lex == "(") type = "LPAREN";
                else if (lex == ")") type = "RPAREN";
                else if (lex == "{") type = "LBRACE";
                else if (lex == "}") type = "RBRACE";
                else if (Regex.IsMatch(lex, $"^(?:{identifiers})$"))
                    type = "ID";

                dt.Rows.Add(lex, type);
            }

            dataGridView1.DataSource = dt;

        }
    }
}

using System.Data;
using System.Text.RegularExpressions;
using Tiny_Language;

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

            inputString = inputString
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&#39;", "'")
                .Replace("&nbsp;", " ");

            string comments = @"/\*[\s\S]*?\*/";
            string keywords = @"\b(int|float|string|read|write|repeat|until|if|elseif|else|then|return|endl|main|end)\b";
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

            List<Token> allTokens = new List<Token>();
            MatchCollection matches = Regex.Matches(inputString, masterPattern);

            foreach (Match m in matches)
            {
                string lex = m.Value.Trim();
                string type = ClassifyToken(lex, comments, keywords, strings,
                                            numbers, boolOps, condOps,
                                            arithOps, funcCall, identifiers);
                allTokens.Add(new Token(lex, type));
            }

            for (int i = 0; i < allTokens.Count - 1; i++)
            {
                bool isId = allTokens[i].Type == "ID";
                bool isMainKeyword = allTokens[i].Type == "KEYWORD"
                                     && allTokens[i].Value == "main";

                if ((isId || isMainKeyword) && allTokens[i + 1].Value == "(")
                    allTokens[i] = new Token(allTokens[i].Value, "FUNC_CALL");
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Lexeme");
            dt.Columns.Add("Token Type");

            foreach (Token t in allTokens)
                dt.Rows.Add(t.Value, t.Type);

            dataGridView1.DataSource = dt;

            if (allTokens.Count == 0)
            {
                MessageBox.Show("No tokens found — nothing to parse.",
                                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Parser parser = new Parser(allTokens);
                parser.ParseProgram();

                MessageBox.Show("Success: Your code is syntactically correct!",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Syntax Error: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ClassifyToken(string lex,
                                     string comments, string keywords,
                                     string strings, string numbers,
                                     string boolOps, string condOps,
                                     string arithOps, string funcCall,
                                     string identifiers)
        {
            if (Regex.IsMatch(lex, $"^(?:{comments})$")) return "COMMENT";
            if (lex == "int" || lex == "float" || lex == "string") return "DATATYPE";
            if (Regex.IsMatch(lex, $"^(?:{keywords})$")) return "KEYWORD";
            if (Regex.IsMatch(lex, $"^(?:{strings})$")) return "STRING";
            if (Regex.IsMatch(lex, $"^(?:{numbers})$")) return "NUM";
            if (lex == ":=") return "ASSIGN";
            if (Regex.IsMatch(lex, $"^(?:{boolOps})$")) return "OP_BOOL";
            if (Regex.IsMatch(lex, $"^(?:{condOps})$")) return "OP_COND";
            if (Regex.IsMatch(lex, $"^(?:{arithOps})$")) return "OP_ARITH";
            if (lex == ";") return "SEMICOLON";
            if (lex == ",") return "COMMA";
            if (lex == "(") return "LPAREN";
            if (lex == ")") return "RPAREN";
            if (lex == "{") return "LBRACE";
            if (lex == "}") return "RBRACE";
            if (Regex.IsMatch(lex, $"^(?:{identifiers})$")) return "ID";
            return "UNKNOWN";
        }
    }
}
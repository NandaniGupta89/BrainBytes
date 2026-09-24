using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QuizApp
{
    public partial class WebForm1 : Page
    {
        // Session keys used to persist quiz state across postbacks
        private const string SESSION_USERNAME = "QuizUserName";
        private const string SESSION_CATEGORY = "QuizCategory";
        private const string SESSION_TIMER_ENABLED = "QuizTimerEnabled";
        private const string SESSION_QUESTIONS = "QuizQuestions";
        private const string SESSION_INDEX = "QuizCurrentIndex";
        private const string SESSION_SCORE = "QuizScore";
        private const string SESSION_RESULTS = "QuizResults";
        private const string SESSION_SECONDS_LEFT = "QuizSecondsLeft";

        private const int SECONDS_PER_QUESTION = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            // pnlSetup is visible by default from markup; nothing to initialize
            // until the user clicks "Start Quiz".
        }

        protected void btnStart_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string userName = txtUserName.Text.Trim();
            string category = rblCategory.SelectedValue;
            bool timerEnabled = chkTimer.Checked;

            Session[SESSION_USERNAME] = userName;
            Session[SESSION_CATEGORY] = category;
            Session[SESSION_TIMER_ENABLED] = timerEnabled;
            Session[SESSION_QUESTIONS] = GetQuestionsForCategory(category);
            Session[SESSION_INDEX] = 0;
            Session[SESSION_SCORE] = 0;
            Session[SESSION_RESULTS] = new List<QuestionResult>();

            pnlSetup.Visible = false;
            pnlQuiz.Visible = true;
            pnlResult.Visible = false;

            ShowQuestion();
        }

        // Safety net: if Session ever expires mid-quiz (e.g. long idle time),
        // bounce back to the setup screen instead of throwing a null reference error.
        private bool EnsureSessionActive()
        {
            if (Session[SESSION_QUESTIONS] == null)
            {
                quizTimer.Enabled = false;
                pnlQuiz.Visible = false;
                pnlResult.Visible = false;
                pnlSetup.Visible = true;
                return false;
            }
            return true;
        }

        private void ShowQuestion()
        {
            if (!EnsureSessionActive())
            {
                return;
            }

            List<Question> questions = (List<Question>)Session[SESSION_QUESTIONS];
            int index = (int)Session[SESSION_INDEX];

            if (index >= questions.Count)
            {
                ShowResults();
                return;
            }

            Question current = questions[index];

            lblUserGreeting.Text = string.Format("Player: {0}  |  Category: {1}", Session[SESSION_USERNAME], Session[SESSION_CATEGORY]);
            lblQuestionNumber.Text = string.Format("Question {0} of {1}", index + 1, questions.Count);
            lblQuestion.Text = current.Text;

            // Progress bar reflects how many questions have been completed so far
            int percent = (int)(((double)index / questions.Count) * 100);
            progressBarFill.Width = Unit.Percentage(percent);

            rblOptions.Items.Clear();
            foreach (string option in current.Options)
            {
                rblOptions.Items.Add(new ListItem(option));
            }
            rblOptions.SelectedIndex = -1;

            btnNext.Text = (index == questions.Count - 1) ? "Submit" : "Next";

            bool timerEnabled = (bool)Session[SESSION_TIMER_ENABLED];
            if (timerEnabled)
            {
                Session[SESSION_SECONDS_LEFT] = SECONDS_PER_QUESTION;
                lblTimer.Text = SECONDS_PER_QUESTION.ToString();
                pnlTimer.Visible = true;
                quizTimer.Enabled = true;
            }
            else
            {
                pnlTimer.Visible = false;
                quizTimer.Enabled = false;
            }
        }

        // Fires once per second (Interval="1000") while quizTimer.Enabled is true
        protected void quizTimer_Tick(object sender, EventArgs e)
        {
            if (!EnsureSessionActive() || Session[SESSION_SECONDS_LEFT] == null)
            {
                return;
            }

            int secondsLeft = (int)Session[SESSION_SECONDS_LEFT] - 1;

            if (secondsLeft <= 0)
            {
                // Time's up on this question — record it as unanswered and auto-advance
                RecordAnswer(-1);
                Session[SESSION_INDEX] = (int)Session[SESSION_INDEX] + 1;
                ShowQuestion();
            }
            else
            {
                Session[SESSION_SECONDS_LEFT] = secondsLeft;
                lblTimer.Text = secondsLeft.ToString();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            if (!EnsureSessionActive())
            {
                return;
            }

            RecordAnswer(rblOptions.SelectedIndex);
            Session[SESSION_INDEX] = (int)Session[SESSION_INDEX] + 1;
            ShowQuestion();
        }

        // selectedIndex is -1 when the timer ran out before the user picked an answer
        private void RecordAnswer(int selectedIndex)
        {
            List<Question> questions = (List<Question>)Session[SESSION_QUESTIONS];
            int index = (int)Session[SESSION_INDEX];
            int score = (int)Session[SESSION_SCORE];
            List<QuestionResult> results = (List<QuestionResult>)Session[SESSION_RESULTS];

            Question current = questions[index];
            bool answered = selectedIndex >= 0;
            bool isCorrect = answered && selectedIndex == current.CorrectIndex;

            if (isCorrect)
            {
                score++;
            }

            results.Add(new QuestionResult
            {
                QuestionNo = index + 1,
                QuestionText = current.Text,
                SelectedAnswer = answered ? current.Options[selectedIndex] : "(no answer — time expired)",
                CorrectAnswer = current.Options[current.CorrectIndex],
                IsCorrect = isCorrect
            });

            Session[SESSION_SCORE] = score;
            Session[SESSION_RESULTS] = results;
        }

        private void ShowResults()
        {
            quizTimer.Enabled = false;
            pnlQuiz.Visible = false;
            pnlResult.Visible = true;

            List<Question> questions = (List<Question>)Session[SESSION_QUESTIONS];
            int score = (int)Session[SESSION_SCORE];
            List<QuestionResult> results = (List<QuestionResult>)Session[SESSION_RESULTS];

            double percentage = questions.Count > 0
                ? Math.Round((double)score / questions.Count * 100, 1)
                : 0;

            lblResultGreeting.Text = string.Format("Great job, {0}!", Session[SESSION_USERNAME]);
            lblScore.Text = string.Format("You scored {0} out of {1} ({2}%)", score, questions.Count, percentage);

            if (percentage >= 80)
            {
                lblRemark.Text = "Excellent work! 🌟";
            }
            else if (percentage >= 50)
            {
                lblRemark.Text = "Good effort — keep practicing! 👍";
            }
            else
            {
                lblRemark.Text = "Keep studying, you'll improve next time! 📘";
            }

            rptReview.DataSource = results;
            rptReview.DataBind();
        }

        protected void btnRestart_Click(object sender, EventArgs e)
        {
            Session.Clear();
            quizTimer.Enabled = false;

            pnlResult.Visible = false;
            pnlQuiz.Visible = false;
            pnlSetup.Visible = true;

            txtUserName.Text = "";
            rblCategory.ClearSelection();
            chkTimer.Checked = false;
        }

        // ---------------- Question Bank (10 per category) ----------------

        private List<Question> GetQuestionsForCategory(string category)
        {
            switch (category)
            {
                case "C":
                    return GetCQuestions();
                case "Java":
                    return GetJavaQuestions();
                case "C#":
                    return GetCSharpQuestions();
                case "Python":
                    return GetPythonQuestions();
                case "ASP.NET":
                    return GetAspNetQuestions();
                default:
                    return GetCSharpQuestions();
            }
        }

        private List<Question> GetCQuestions()
        {
            return new List<Question>
            {
                new Question { Text = "Which header file is required for standard input/output functions in C?", Options = new List<string> { "stdio.h", "conio.h", "stdlib.h", "string.h" }, CorrectIndex = 0 },
                new Question { Text = "Which operator is used to access the value stored at a given address in C?", Options = new List<string> { "&", "*", "%", "#" }, CorrectIndex = 1 },
                new Question { Text = "What is the correct syntax to declare a pointer to an integer in C?", Options = new List<string> { "int ptr;", "int *ptr;", "ptr int;", "pointer int ptr;" }, CorrectIndex = 1 },
                new Question { Text = "Which function is used to allocate memory dynamically in C?", Options = new List<string> { "malloc()", "new", "alloc()", "create()" }, CorrectIndex = 0 },
                new Question { Text = "What is the typical size of an int on a modern 32-bit or 64-bit C compiler?", Options = new List<string> { "2 bytes", "4 bytes", "8 bytes", "1 byte" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword is used to define a constant value in C?", Options = new List<string> { "final", "const", "static", "readonly" }, CorrectIndex = 1 },
                new Question { Text = "Which function is used to print formatted output to the console in C?", Options = new List<string> { "print()", "printf()", "echo()", "display()" }, CorrectIndex = 1 },
                new Question { Text = "What does the 'break' statement do inside a loop in C?", Options = new List<string> { "Skips the current iteration only", "Exits the loop entirely", "Restarts the loop from the top", "Pauses the loop temporarily" }, CorrectIndex = 1 },
                new Question { Text = "Which of these is a valid loop keyword in standard C?", Options = new List<string> { "for", "foreach", "repeat", "loop" }, CorrectIndex = 0 },
                new Question { Text = "According to the C standard, what is the return type of the main() function?", Options = new List<string> { "void", "int", "char", "float" }, CorrectIndex = 1 }
            };
        }

        private List<Question> GetJavaQuestions()
        {
            return new List<Question>
            {
                new Question { Text = "Which keyword is used to inherit a class in Java?", Options = new List<string> { "implements", "extends", "inherits", "base" }, CorrectIndex = 1 },
                new Question { Text = "Which method serves as the entry point of a standalone Java application?", Options = new List<string> { "start()", "main()", "run()", "init()" }, CorrectIndex = 1 },
                new Question { Text = "Which of these is NOT a Java primitive data type?", Options = new List<string> { "int", "boolean", "String", "char" }, CorrectIndex = 2 },
                new Question { Text = "Which keyword is used to create a new object instance in Java?", Options = new List<string> { "create", "new", "object", "make" }, CorrectIndex = 1 },
                new Question { Text = "What is the default value of an uninitialized boolean field in Java?", Options = new List<string> { "true", "false", "0", "null" }, CorrectIndex = 1 },
                new Question { Text = "Which collection class allows duplicate elements and preserves insertion order?", Options = new List<string> { "HashSet", "ArrayList", "TreeSet", "HashMap" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword prevents a class from being subclassed in Java?", Options = new List<string> { "static", "final", "private", "sealed" }, CorrectIndex = 1 },
                new Question { Text = "Which construct is used to handle runtime exceptions in Java?", Options = new List<string> { "if-else", "try-catch", "switch-case", "for-loop" }, CorrectIndex = 1 },
                new Question { Text = "Which access modifier restricts a member to be visible only within its own class?", Options = new List<string> { "public", "protected", "private", "default" }, CorrectIndex = 2 },
                new Question { Text = "Which construct allows a Java class to achieve multiple inheritance of type?", Options = new List<string> { "class", "interface", "abstract class", "package" }, CorrectIndex = 1 }
            };
        }

        private List<Question> GetCSharpQuestions()
        {
            return new List<Question>
            {
                new Question { Text = "What is the correct file extension for a C# source file?", Options = new List<string> { ".java", ".cs", ".cpp", ".vb" }, CorrectIndex = 1 },
                new Question { Text = "Which symbol is used to inherit from a base class in C#?", Options = new List<string> { "extends", "implements", ": (colon)", "inherits" }, CorrectIndex = 2 },
                new Question { Text = "Which collection type stores data as key-value pairs in C#?", Options = new List<string> { "ArrayList", "List<T>", "Dictionary<TKey,TValue>", "Stack" }, CorrectIndex = 2 },
                new Question { Text = "What does ASP.NET use to preserve control values between postbacks on the same page?", Options = new List<string> { "Cookies only", "ViewState", "Local Storage", "Cache only" }, CorrectIndex = 1 },
                new Question { Text = "Which event fires first in the ASP.NET Web Forms page life cycle?", Options = new List<string> { "Page_Load", "Page_Init", "Page_PreRender", "Page_Unload" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword prevents a class from being inherited in C#?", Options = new List<string> { "final", "sealed", "const", "readonly" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword is used to catch an exception in C#?", Options = new List<string> { "except", "catch", "handle", "rescue" }, CorrectIndex = 1 },
                new Question { Text = "What is the default access level of a class member in C# when none is specified?", Options = new List<string> { "public", "protected", "private", "internal" }, CorrectIndex = 2 },
                new Question { Text = "Which symbol makes a value type nullable in C#, e.g. int? x?", Options = new List<string> { "!", "?", "~", "@" }, CorrectIndex = 1 },
                new Question { Text = "Which LINQ method filters a sequence based on a condition?", Options = new List<string> { "Select", "Where", "OrderBy", "GroupBy" }, CorrectIndex = 1 }
            };
        }

        private List<Question> GetPythonQuestions()
        {
            return new List<Question>
            {
                new Question { Text = "Which symbol is used to start a single-line comment in Python?", Options = new List<string> { "//", "#", "/* */", "--" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword is used to define a function in Python?", Options = new List<string> { "function", "def", "func", "define" }, CorrectIndex = 1 },
                new Question { Text = "Which built-in type is used to store text (a sequence of characters) in Python?", Options = new List<string> { "char", "string", "str", "text" }, CorrectIndex = 2 },
                new Question { Text = "Which of these Python data types is mutable?", Options = new List<string> { "tuple", "string", "list", "int" }, CorrectIndex = 2 },
                new Question { Text = "How does Python mark the start and end of a block of code, such as inside an if statement?", Options = new List<string> { "Curly braces", "Indentation", "Parentheses", "Semicolons" }, CorrectIndex = 1 },
                new Question { Text = "Which built-in function returns the number of items in a list?", Options = new List<string> { "length()", "len()", "size()", "count()" }, CorrectIndex = 1 },
                new Question { Text = "Which keyword is used to catch an exception in Python?", Options = new List<string> { "catch", "except", "rescue", "handle" }, CorrectIndex = 1 },
                new Question { Text = "Which standard module is commonly used for regular expressions in Python?", Options = new List<string> { "regex", "re", "pyregex", "string" }, CorrectIndex = 1 },
                new Question { Text = "In a Python class method, what does the 'self' parameter refer to?", Options = new List<string> { "The class itself", "The instance the method was called on", "The parent class", "A global variable" }, CorrectIndex = 1 },
                new Question { Text = "Which syntax is used to create a list in Python?", Options = new List<string> { "Curly braces {}", "Parentheses ()", "Square brackets []", "Angle brackets <>" }, CorrectIndex = 2 }
            };
        }

        private List<Question> GetAspNetQuestions()
        {
            return new List<Question>
            {
                new Question { Text = "Which file is used to store application-wide configuration settings in ASP.NET?", Options = new List<string> { "App.config", "Web.config", "Global.asax", "Settings.xml" }, CorrectIndex = 1 },
                new Question { Text = "Which ASP.NET Web Forms control is best suited for displaying tabular data with built-in paging and sorting?", Options = new List<string> { "Repeater", "DataList", "GridView", "ListView" }, CorrectIndex = 2 },
                new Question { Text = "Which object is used to store data specific to one user across multiple requests in ASP.NET?", Options = new List<string> { "Application", "Session", "ViewState", "Cache" }, CorrectIndex = 1 },
                new Question { Text = "Which event handler runs code each time an ASP.NET Web Forms page loads?", Options = new List<string> { "Page_Init", "Page_Load", "Page_PreRender", "Page_Render" }, CorrectIndex = 1 },
                new Question { Text = "Which directive at the top of an .aspx file links it to its code-behind class?", Options = new List<string> { "<%@ Import %>", "<%@ Page %>", "<%@ Register %>", "<%@ Control %>" }, CorrectIndex = 1 },
                new Question { Text = "Which validator control ensures a user cannot leave a required field empty?", Options = new List<string> { "CompareValidator", "RequiredFieldValidator", "RangeValidator", "RegularExpressionValidator" }, CorrectIndex = 1 },
                new Question { Text = "What is the primary purpose of the Global.asax file in an ASP.NET application?", Options = new List<string> { "Store connection strings", "Handle application-level events", "Define the page layout", "Store user session data" }, CorrectIndex = 1 },
                new Question { Text = "Which method is commonly used to send the user's browser to another page in ASP.NET?", Options = new List<string> { "Server.Transfer()", "Response.Redirect()", "Page.Redirect()", "Request.Redirect()" }, CorrectIndex = 1 },
                new Question { Text = "Which ASP.NET AJAX control allows part of a page to refresh without a full postback?", Options = new List<string> { "ScriptManager", "UpdatePanel", "Timer", "AjaxPanel" }, CorrectIndex = 1 },
                new Question { Text = "What is the .aspx markup made of, alongside standard HTML, in a Web Forms page?", Options = new List<string> { "XAML elements", "Server controls (asp: tags)", "JSX elements", "Razor syntax" }, CorrectIndex = 1 }
            };
        }
    }

    [Serializable]
    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
    }

    [Serializable]
    public class QuestionResult
    {
        public int QuestionNo { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}

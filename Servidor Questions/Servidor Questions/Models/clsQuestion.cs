using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servidor_Questions.Models
{
    public class clsQuestion
    {
        public String category { get; set; }
        public String type { get; set; }
        public String difficulty { get; set; }
        public String question { get; set; }
        public String correct_answer { get; set; }
        public String[] incorrect_answers { get; set; }

        public bool alreadyPlayed { get; set; }

        public clsQuestion(string category, string type, string difficulty, string question, string correct_answer, string[] incorrect_answers)
        {
            this.category = category;
            this.type = type;
            this.difficulty = difficulty;
            this.question = question;
            this.correct_answer = correct_answer;
            this.incorrect_answers = incorrect_answers;
            this.alreadyPlayed = false;
        }
    }
}
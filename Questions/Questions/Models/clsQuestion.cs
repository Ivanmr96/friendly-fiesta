using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Questions.Models
{
    public class clsQuestion : INotifyPropertyChanged, IQuestion
    {
        public String category { get; set; }
        public String type { get; set; }
        public String difficulty { get; set; }
        public String question { get; set; }
        public String correct_answer { get; set; }
        private String[] _incorrect_answers { get; set; }
        private String[] _answers { get; set; }

        public clsQuestion() { }

        public clsQuestion(string category, string type, string difficulty, string question, string correct_answer, string[] incorrect_answers)
        {
            this.category = category;
            this.type = type;
            this.difficulty = difficulty;
            this.question = question;
            this.correct_answer = correct_answer;
            this._incorrect_answers = incorrect_answers;

            //"baraja" las respuestas
            String[] answersArray = { incorrect_answers[0], incorrect_answers[1], incorrect_answers[2], correct_answer };
            Random rand = new Random();
            _answers = answersArray.OrderBy(x => rand.Next()).ToArray();

        }

        public String[] incorrect_answers
        {
            get { return _incorrect_answers; }
            set
            {
                _incorrect_answers = value;
                NotifyPropertyChanged("incorrect_answers");
            }
        }

        public String[] answers
        {
            get
            {
                if(_answers == null)
                {
                    String[] answersArray = { incorrect_answers[0], incorrect_answers[1], incorrect_answers[2], correct_answer };
                    Random rand = new Random();
                    _answers = answersArray.OrderBy(x => rand.Next()).ToArray();
                }

                return _answers;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
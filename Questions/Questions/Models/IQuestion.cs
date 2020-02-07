using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Models
{
    public interface IQuestion
    {
        String category { get; set; }
        String type { get; set; }
        String difficulty { get; set; }
        String question { get; set; }
        String correct_answer { get; set; }
        String[] incorrect_answers { get; set; }
        String[] answers { get; }
    }
}

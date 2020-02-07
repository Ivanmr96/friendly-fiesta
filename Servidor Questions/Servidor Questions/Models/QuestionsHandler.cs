using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Servidor_Questions.Models
{
    public class QuestionsHandler
    {
        /// <summary>
        /// Obtiene la uri base de la API
        /// </summary>
        /// <returns>La uri base de la API</returns>
        public static String getUriBase()
        {
            return "https://opentdb.com/api.php?";
        }

        /// <summary>
        /// Devuelve un numero de preguntas determinados de una categoria, dificultad y tipo dado
        /// </summary>
        /// <param name="numberOfQuestions">El número de preguntas que se desea obtener</param>
        /// <param name="category">La categoria de las preguntas</param>
        /// <param name="difficulty">La dificultad de las preguntas</param>
        /// <param name="type">El tipo de preguntas, puede ser tipo opcion múltipe o tipo verdadero/falso</param>
        /// <returns></returns>
        public async static Task<List<clsQuestion>> getQuestions(int numberOfQuestions, String category, String difficulty, String type)
        {
            //Lista donde irán las preguntas
            List<clsQuestion> questions = new List<clsQuestion>();

            //Coge la URI base y le añade los querystring necesarios
            String uri = getUriBase();
            uri += "amount=" + numberOfQuestions;
            uri += "&category=" + category;
            //uri += "&difficulty=" + difficulty;
            uri += "&type=" + type;

            HttpClient cliente = new HttpClient();

            try
            {
                HttpResponseMessage response = await cliente.GetAsync(new Uri(uri));

                //Guarda el resultado obtenido en un string
                string result = await response.Content.ReadAsStringAsync();

                //Convierte el string del resultado en un objeto JSON
                JObject json = JObject.Parse(result);

                //Guarda todos los hijos que están dentro del array "results"
                IList <JToken> tokens = json["results"].Children().ToList();

                //Recorre todos los tokens hijos, los convierte en objeto clsQuestion y los guarda en la lista
                clsQuestion question;
                foreach(JToken token in tokens)
                {
                    //Convierte el token en clsQuestion
                    question = token.ToObject<clsQuestion>();

                    //decodofica los strings del objeto por si contienen algún código especial HTML.
                    question.question = HttpUtility.HtmlDecode(question.question);
                    question.correct_answer = HttpUtility.HtmlDecode(question.correct_answer);
                    for(int i = 0; i < question.incorrect_answers.Length; i++)
                    {
                        question.incorrect_answers[i] = HttpUtility.HtmlDecode(question.incorrect_answers[i]);
                    }
                    //Añade el objeto a la lista de preguntas
                    questions.Add(question);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return questions;
        }
    }
}
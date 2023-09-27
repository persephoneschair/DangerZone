using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

public static class QuestionManager
{
    public static Pack currentPack = null;
    public static List<Question> tiebreakerQuestions = new List<Question>();
    public static int nextTiebreakerQuestion = 0;

    public static void DecompilePack(TextAsset tx, bool oldFormat)
    {
        tiebreakerQuestions = JsonConvert.DeserializeObject<List<Question>>(Operator.Get.tiebreakerQuestions.text);
        nextTiebreakerQuestion = PlayerPrefs.GetInt("tb");

        if (oldFormat)
            ConvertFromOld(tx);
        else
            currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);

        if (!currentPack.preserveQuestionOrder)
            foreach (Round r in currentPack.rounds)
                r.questions.Shuffle();

        if (!currentPack.preserveRoundOrder)
        {
            Round lastRound = currentPack.rounds.LastOrDefault();
            currentPack.rounds.Remove(lastRound);
            currentPack.rounds.Shuffle();
            currentPack.rounds.Add(lastRound);
        }

        foreach (Round r in currentPack.rounds)
            foreach (Question q in r.questions)
                q.answers.Shuffle();

        /*foreach(Round r in currentPack.rounds)
        {
            DebugLog.Print(r.category, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
            foreach(Question q in r.questions)
            {
                DebugLog.Print($"{((Array.IndexOf(r.questions.ToArray(), q) + 1).ToString())}) {q.questionText}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
                foreach (Answer a in q.answers)
                    DebugLog.Print(a.answerText + " (" + (a.isCorrect ? "CORRECT" : "INCORRECT") + ")", DebugLog.StyleOption.Italic, a.isCorrect ? DebugLog.ColorOption.Green : DebugLog.ColorOption.Red);
            }
        }*/
    }

    public static int GetRoundQCount()
    {
        return currentPack.rounds[GameplayManager.Get.roundsPlayed].questions.Count();
    }

    public static Question GetQuestion(int qNum)
    {
        return currentPack.rounds[GameplayManager.Get.roundsPlayed].questions[qNum];
    }

    public static Question GetTiebreakerQuestion()
    {
        int thisQ = nextTiebreakerQuestion;
        nextTiebreakerQuestion = (nextTiebreakerQuestion + 1) % tiebreakerQuestions.Count;
        if (!Operator.Get.testMode)
            PlayerPrefs.SetInt("tb", nextTiebreakerQuestion);

        return tiebreakerQuestions[thisQ];
    }

    public static bool IsFinalRound()
    {
        return GameplayManager.Get.roundsPlayed + 1 == currentPack.rounds.Count ? true : false;
    }

    public static void ConvertFromOld(TextAsset oldJson)
    {
        QuestionPackOld oldPack = JsonConvert.DeserializeObject<QuestionPackOld>(oldJson.text);

        currentPack = new Pack
        {
            author = oldPack.author,
            preserveRoundOrder = oldPack.preserveRoundOrder,
            preserveQuestionOrder = oldPack.preserveQuestionOrder
        };

        Round r = null;
        for (int i = 0; i < oldPack.questions.Count; i++)
        {
            if(i % 5 == 0)
            {
                if(r != null)
                    currentPack.rounds.Add(r);

                r = new Round()
                {
                    category = oldPack.questions[i].category
                };
            }

            if (string.IsNullOrEmpty(oldPack.questions[i].question))
                continue;

            Question qu = new Question()
            {
                questionText = oldPack.questions[i].question
            };
            Answer an = new Answer();
            an.answerText = oldPack.questions[i].correct;
            an.isCorrect = true;
            qu.answers.Add(an);
            for (int p = 0; p < 3; p++)
            {
                Answer inAn = new Answer()
                {
                    answerText = oldPack.questions[i].incorrectAnswers[p],
                    isCorrect = false
                };
                qu.answers.Add(inAn);
            }
            r.questions.Add(qu);
        }
        currentPack.rounds.Add(r);
    }
}

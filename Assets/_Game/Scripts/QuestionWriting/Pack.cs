using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Pack
{
    public string author;
    public bool preserveRoundOrder;
    public bool preserveQuestionOrder;
    public List<Round> rounds = new List<Round>();
}

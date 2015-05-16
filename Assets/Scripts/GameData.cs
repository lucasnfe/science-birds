using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData : ABSingleton<GameData> {
		
	private string _url = "http://52.0.243.131:8080/ABQuestionaryServer/services/QuestionaryServer";

	public string Age          { get; set; }
	public string Gender 	   { get; set; }
	public string GameXP       { get; set; }
	public string AngryBirdsXP { get; set; }
	public string Education    { get; set; }
	
	public string LevelGroupA  		 { get; set; }
	public string LevelGroupB  		 { get; set; }
	public string CurrentQuestionary { get; set; }

	public string AnswersTurinTest               { get; set; }
	public string []AnswersImmersionQuestionaryA { get; set; }
	public string []AnswersImmersionQuestionaryB { get; set; }
	public string []AnswersSimilarityQuestionary { get; set; }

	public string GetGeneratedGroupName() {

		string groupName = "";

		if(LevelGroupA == "GeneratedLevels")
			groupName = "Group A";

		if(LevelGroupB == "GeneratedLevels")
			groupName = "Group B";

		return groupName;
	}

	public void ShuffleLevelGroups() {

		if(Random.value < 0.5) 
		{
			LevelGroupA = "GeneratedLevels";
			LevelGroupB = "RovioLevels";
		}
		else
		{
			LevelGroupA = "RovioLevels";
			LevelGroupB = "GeneratedLevels";
		}
	}

	public WWW CreateRequest() {

		string envelopeAnswersA = "";
		foreach(string ans in AnswersImmersionQuestionaryA)
			envelopeAnswersA += "<intf:answersImmersionQuestA>" + LevelGroupA + "-" + ans + "</intf:answersImmersionQuestA>";

		string envelopeAnswersB = "";
		foreach(string ans in AnswersImmersionQuestionaryB)		
			envelopeAnswersB += "<intf:answersImmersionQuestB>" + LevelGroupB + "-" + ans + "</intf:answersImmersionQuestB>";

		string envelopeAnswersS = "";
		foreach(string ans in AnswersSimilarityQuestionary)
			envelopeAnswersS += "<intf:anwswersSimilarityQuest>" + ans + "</intf:anwswersSimilarityQuest>";

		string envelope =  "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" +
        "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:apachesoap=\"http://xml.apache.org/xml-soap\" xmlns:impl=\"http://questionaryserver.angrybirdsendless.org\" xmlns:intf=\"http://questionaryserver.angrybirdsendless.org\" xmlns:wsdl=\"http://schemas.xmlsoap.org/wsdl/\" xmlns:wsdlsoap=\"http://schemas.xmlsoap.org/wsdl/soap/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" >" + 
		"<SOAP-ENV:Body><intf:saveQuestionary xmlns:intf=\"http://questionaryserver.angrybirdsendless.org\">" + 
		"<intf:age>" + Age + "</intf:age>" +
		"<intf:gender>" + Gender + "</intf:gender>" +
		"<intf:gameXP>" + GameXP + "</intf:gameXP>" + 
		"<intf:angryBirdsXP>" + AngryBirdsXP + "</intf:angryBirdsXP>" + 
		"<intf:education>" + Education + "</intf:education>" + 
		"<intf:answerTurinTest>" + AnswersTurinTest + "-" + GetGeneratedGroupName() + "</intf:answerTurinTest>" + 
		envelopeAnswersA + envelopeAnswersB + envelopeAnswersS +  
		"</intf:saveQuestionary></SOAP-ENV:Body></SOAP-ENV:Envelope>";
				 
		Dictionary<string,string> headers = new Dictionary<string,string>();

		//headers["Host"] = "52.0.243.131";
		headers["Content-Type"] = "text/xml; charset=utf-8";
		//headers["Content-Length"] = envelope.Length.ToString();
		headers["SOAPAction"] = "\"" + _url + "/saveQuestionary\"";

		Debug.Log(envelope);

		return new WWW (_url, System.Text.Encoding.UTF8.GetBytes(envelope), headers);
	}

	public override string ToString() {

		string dataString = "";

		dataString += "Age: " + Age + "\n";
		dataString += "Gender: " + Gender + "\n";
		dataString += "GameXP: " + GameXP + "\n";
		dataString += "AngryBirdsXP: " + AngryBirdsXP + "\n";
		dataString += "Education: " + Education + "\n";
		dataString += "Turin Test: " + AnswersTurinTest + "\n";

		if(AnswersImmersionQuestionaryA != null)
		for(int i = 0; i < AnswersImmersionQuestionaryA.Length; i++)
			dataString += "QuestA-" + i + ": " + AnswersImmersionQuestionaryA[i] + "\n";
	
		if(AnswersImmersionQuestionaryB != null)
		for(int i = 0; i < AnswersImmersionQuestionaryB.Length; i++)
			dataString += "QuestB-" + i + ": " + AnswersImmersionQuestionaryB[i] + "\n";

		if(AnswersSimilarityQuestionary != null)
		for(int i = 0; i < AnswersSimilarityQuestionary.Length; i++)
			dataString += "QuestSim-" + i + ": " + AnswersSimilarityQuestionary[i] + "\n";

		return dataString;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** \class GameData
 *  \brief  Handles division of different level groups and request for questionary.
 * 
 *  Selects the names of the generated and rovio level groups, shuffle the groups, create request for questionary and converts the questionary attributes to string
 */
public class GameData : ABSingleton<GameData> {
		
    /** URL for the questionary server*/
	private string _url = "http://52.0.243.131:8080/ABQuestionaryServer/services/QuestionaryServer";

    /**Age of the user*/
	public string Age          { get; set; }
    /**Gender of the user*/
    public string Gender 	   { get; set; }
    /**Game Experience level of the user*/
    public string GameXP       { get; set; }
    /**Angry Birds Exeperience level of the user*/
    public string AngryBirdsXP { get; set; }
    /**Education level of the user*/
    public string Education    { get; set; }

    /**Name of the origin of levels at group A (GeneratedLevels or RovioLevels)*/
    public string LevelGroupA  		 { get; set; }
    /**Name of the origin of levels at group B (GeneratedLevels or RovioLevels)*/
    public string LevelGroupB  		 { get; set; }
    /**Current Questionary being used*/
    public string CurrentQuestionary { get; set; }

    /**User's answer to Turing Test*/
    public string AnswersTurinTest               { get; set; }
    /**User's answers to Immersion Questionary for group A of levels*/
    public string []AnswersImmersionQuestionaryA { get; set; }
    /**User's answers to Immersion Questionary for group B of levels*/
    public string []AnswersImmersionQuestionaryB { get; set; }
    /**User's answers to Similarity Questionary for levels in same group*/
    public string []AnswersSimilarityQuestionary { get; set; }

    /**
     *  Get which group the Generated Levels are in
     *  @return string  "Group A" if Generated Levels are on LevelGroupA , "Group B" if they are on LevelGroupB
     */
    public string GetGeneratedGroupName() {

		string groupName = "";

		if(LevelGroupA == "GeneratedLevels")
			groupName = "Group A";

		if(LevelGroupB == "GeneratedLevels")
			groupName = "Group B";

		return groupName;
	}
    /**
     *  Shuffle the Generated and Rovio level groups between LevelGroup A and B with equal probability
     */
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
    /**
     *  Creates a request for the questionary
     *  Contains information about Immersion Questionary A and B, Similarity Questionary, User's Age, Gender,
     *  Game Experience, Angry Birds Experience, Education level, Turing Test and header information
     *  @return WWW object created with url, all the information from the questionary, and headers
     */
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

    /**
     *  Overrides ToString method to convert information about Age, Gender, Game Experience, Angry Birds Experience, Education Level,
     *  Turing Test, Immersion Questionaries answers and Immersion Questionary answer to a single string.
     * @return string   String containing all the information about the questionaries
     */
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

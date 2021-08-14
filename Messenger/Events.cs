using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Events
{

	public class TestEvent
	{
		public static readonly string Test = "TestEvent.Test";      //示例
	}

	public class Settings
	{
		public static readonly string FullScreen = "Settings.FullScreen";
		public static readonly string Window = "Settings.Window";
		public static readonly string MiniMize = "Settings.MiniMize";
		public static readonly string Quit = "Settings.Quit";
		public static readonly string BackHome = "Settings.BackHome";
		public static readonly string StopLaoding = "Settings.StopLaoding";
	}

	public class Exam
	{
		public static readonly string ClickNode = "Exam.ClickNode";
		public static readonly string ClickExam = "Exam.ClickExam";
		public static readonly string ClickOption = "Exam.ClickOption";
		public static readonly string NextTopic = "Exam.NextTopic";
		public static readonly string LastTopic = "Exam.LastTopic";
		public static readonly string ExamEnd = "Exam.ExamEnd";
		public static readonly string Analysis = "Exam.Analysis";
	}

	public class ModelEvent
	{
		public static readonly string SwitchLevel = "ModelEvent.SwitchLevel";
	}
}



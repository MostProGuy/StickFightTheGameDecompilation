using System;

public class DontBeRude
{
	private string[] m_badwords0 = new string[14]
	{
		"Nigger", "niggah", "Neger", "Niger", "Negro", "Chink", "Cholo", "Coon", "gypsy", "kike",
		"niglet", "paki", "spick", "spig"
	};

	private string[] m_badwords1 = new string[7] { "Fag", "faggot", "queer", "gay", "dyke", "homo", "sissy" };

	private string[] m_badwords2 = new string[2] { "tranny", "shemale" };

	private string[] m_badwords3 = new string[6] { "whore", "bitch", "cunt", "slut", "pussy", "well actually" };

	private string[] m_badwords4 = new string[6] { "retard", "fucktard", "mongoloid", "midget", "retarded", "tard" };

	private const int SIZE = 5;

	private string[][] m_badWordsCollection = new string[5][];

	private string[] m_goodWordsCollection = new string[5] { "I'm a bit racist to be honest", "Its great that people can choose who to love", "Transphobia isn't cool", "I'm a bit sexist, I should call my mother", "Everyone is different and thats fucking cool!" };

	public DontBeRude()
	{
		Array.Sort(m_badwords0);
		Array.Sort(m_badwords1);
		Array.Sort(m_badwords2);
		Array.Sort(m_badwords3);
		Array.Sort(m_badwords4);
		m_badWordsCollection[0] = m_badwords0;
		m_badWordsCollection[1] = m_badwords1;
		m_badWordsCollection[2] = m_badwords2;
		m_badWordsCollection[3] = m_badwords3;
		m_badWordsCollection[4] = m_badwords4;
	}

	public void CensorString(ref string s)
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < m_badWordsCollection[i].Length; j++)
			{
				if (s.ToLower().Contains(m_badWordsCollection[i][j].ToLower()))
				{
					s = m_goodWordsCollection[i];
					return;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace htmlParser
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		DataTable dt;
		

		  private DataTable Processing(string html)   //  библиотека для парсинга 
		{
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);


			var nodes = doc.DocumentNode.SelectNodes("//table[@class='standings']/tr");
			var hrefs = doc.DocumentNode.SelectNodes("//a[contains(@class,'rated-user')]"); //node.GetAttributeValue("href", null)


			string contest = doc.DocumentNode.SelectSingleNode("//div[@class='contest-name']/a").InnerText.Trim();
			DataTable novDataTable2 = new DataTable(contest);
			

			var headers = nodes[0].Elements("th").Select(th => th.InnerText.Trim());

			string novHead = "";
			foreach (var header in headers)
			{if (header.Equals("Who"))
				{
					novHead = "Никнейм";
					novDataTable2.Columns.Add(novHead);
					novHead = "Настоящее имя";
					novDataTable2.Columns.Add(novHead);
				}
				if (header.Equals("="))
				{
					novHead = "Рейтинг";
					novDataTable2.Columns.Add(novHead);
				}
				

				

				
			}
			
			var rows = nodes.Skip(1).Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim().Replace("&nbsp;", "")).ToArray());


			HtmlAgilityPack.HtmlDocument docAcc = new HtmlAgilityPack.HtmlDocument();

			string[] NSP = new string[rows.Count() - 1]; 


			int i = 0;
			foreach (var href in hrefs)
			{
				string accountUrl = "https://codeforces.com" + href.Attributes["href"].Value;
				accountUrl = loadFromURL(accountUrl);
				docAcc.LoadHtml(accountUrl);

				var nodesInfo = docAcc.DocumentNode.SelectNodes("//div[contains(@class,'main-info')]/div/div");
				
				if (nodesInfo == null)
				{
					NSP[i] = "Никто не знает кто это, но он участник";
				}
				else
				{NSP[i] = nodesInfo[0].InnerText.Split(',')[0];
					
				}

				i=i+1;


			}

			i = 0;
			foreach (var row in rows)
			{
				if (!row[1].Contains("Accepted"))
				{
					
						string[] newRow = new string[3];
						
						newRow[0] = row[1];
						newRow[1] = NSP[i];
						
						newRow[2] = row[2];
						

						novDataTable2.Rows.Add(newRow);
						i=i+1;
					
				}
			}

			return novDataTable2;
		}

		private string loadFromURL(string url)
		{
			string result_html;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())


			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				result_html = reader.ReadToEnd();
			}

			return result_html;
			
		}


         private void Form1_Load(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Maximized;
            dt = new DataTable();
		}



		private void buttonLoadFromURL_Click(object sender, EventArgs e)
		{
			string url = textBoxURL.Text;

			string html = loadFromURL(url);

			
			dt = Processing(html);
			dataGridViewResults.DataSource = dt;
			dataGridViewResults.AllowUserToAddRows = false;
			dataGridViewResults.AllowUserToResizeRows = false;
			
			dataGridViewResults.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			

		}

		private void buttonLoadFromURL_MouseDown(object sender, MouseEventArgs e)
		{
		

		}

		

		
		private void помощьToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Pomosh help = new Pomosh();
			help.ShowDialog();
		}

		private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Oprogramme prog = new Oprogramme();
			prog.ShowDialog();

		}

		private void выходToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonSort_Click(object sender, EventArgs e) //сортировка
		{
			dataGridViewResults.Sort(dataGridViewResults.Columns[3], ListSortDirection.Ascending);
		}

		

		

		private void добавитьСоревнованиеToolStripMenuItem_Click(object sender, EventArgs e)
		{
			
				Dobavit add = new Dobavit();
				add.ShowDialog();
				string url2 = add.stroka;
				add.Close();
				string dop_html = loadFromURL(url2);

				DataTable dopTable = Processing(dop_html); // метод кот парсит таблицу 

				string contest = dopTable.TableName;
				dt.Columns.Add(contest);
                for (int i = 0; i < dopTable.Rows.Count; i++)
                {
					string nik1 = dopTable.Rows[i][0].ToString();
					bool exist = false;
					for (int j = 0; j < dt.Rows.Count; j++)
					{
						string nik2 = dt.Rows[j][0].ToString();
						if (nik1.Equals(nik2))
						{
							dt.Rows[j][dt.Columns.Count - 1] = dopTable.Rows[i][2];
							exist = true;
							break;
						}

					}
					if (!exist )
					{
						string[] newRow = new string[dt.Columns.Count];
						newRow[0] = dopTable.Rows[i][0].ToString();
						newRow[1] = dopTable.Rows[i][1].ToString();
						newRow[dt.Columns.Count - 1] = dopTable.Rows[i][2].ToString();
						dt.Rows.Add(newRow);
					}
				}

				
			
		
				

				dataGridViewResults.DataSource = dt;
			

		}

		private void удалитьСоревнованиеToolStripMenuItem_Click(object sender, EventArgs e)
		{   dt = new DataTable();
			dataGridViewResults.DataSource = dt;

		}

		private void dataGridViewResults_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			int index = e.RowIndex;
			string indexStr = (index + 1).ToString();
			object header = this.dataGridViewResults.Rows[index].HeaderCell.Value;
			if (header == null || !header.Equals(indexStr))
				this.dataGridViewResults.Rows[index].HeaderCell.Value = indexStr;
		}
	}
}

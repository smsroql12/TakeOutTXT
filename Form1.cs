using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TakeOutTXT
{
    public partial class Form1 : Form
    {
        string[] lines; // 텍스트 값(줄 단위)이 복사되어 넣어질 배열
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView2.Columns[0].DisplayIndex = 0;
            listView2.Columns[1].DisplayIndex = 1;
            listView2.Columns[2].DisplayIndex = 2;
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "파일 불러오기";
            ofd.Filter = "텍스트 파일 (*.txt) | *.txt; |모든 파일 (*.*) | *.*";
            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                listView1.Items.Clear();
                string path = ofd.FileName;
                txtFilePath.Text = path;
                
                var list = new List<string>();
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.Default))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                        list.Add(line);
                }
                lines = list.ToArray(); //배열 복사

                for (int i = 0; i < lines.Length; i++)
                {
                    ListViewItem lvi = new ListViewItem((i + 1).ToString());
                    lvi.SubItems.Add(lines[i]);
                    listView1.Items.Add(lvi);
                }
                richTextBox1.Text = null;
                listView2.Items.Clear();
            }
        }

        private void btnKeywordSearch_Click(object sender, EventArgs e)
        {
            if (txtFilePath.Text == null || txtFilePath.Text == "")
            {
                MessageBox.Show("먼저 '불러오기' 버튼을 클릭하여\n텍스트 파일을 로드 해 주세요.");
                return;
            }
            else if (txtKeyword.Text == null || txtKeyword.Text == "")
            {
                MessageBox.Show("검색 할 키워드를 입력 해 주세요.");
                txtKeyword.Focus();
                return;
            }
            else
            {
                var tempLines = lines.ToArray();
                int i = 1, counter = 1; // i -> 인덱스(개수), counter -> 참조 인덱스
                listView2.Items.Clear();
                foreach (var line in tempLines)
                {
                    if (caseCheck.Checked == true)
                    {
                        if (line.Contains(txtKeyword.Text, StringComparison.CurrentCulture))
                        {
                            ListViewItem lvi = new ListViewItem(i.ToString()); // 인덱스
                            lvi.SubItems.Add(counter.ToString()); // 참조 인덱스
                            lvi.SubItems.Add(line); // 내용
                            listView2.Items.Add(lvi);
                            i++;
                        }
                    }
                    else
                    {
                        if (line.Contains(txtKeyword.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            ListViewItem lvi = new ListViewItem(i.ToString()); // 인덱스
                            lvi.SubItems.Add(counter.ToString()); // 참조 인덱스
                            lvi.SubItems.Add(line); // 내용
                            listView2.Items.Add(lvi);
                            i++;
                        }
                    }
                    ++counter;
                }
            }
        }
        
        private void btnSaveTxT_Click(object sender, EventArgs e)
        {
            List<string> txtExport = new List<string>();
            for (int i = 0; i < listView2.Items.Count; ++i) 
                txtExport.Add(listView2.Items[i].SubItems[1].Text + " >>> " + listView2.Items[i].SubItems[2].Text);
            
            string fileName;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "저장 경로를 지정 하세요.";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "텍스트 파일 (*.txt) | *.txt |모든 파일 (*.*) | *.*";
            saveFileDialog.FileName = "textFile";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                try
                {
                    File.WriteAllLines(fileName, txtExport);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else   return;
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                if (listView2.SelectedItems[0].SubItems[0] != null)
                {
                    richTextBox1.Text = null;
                    richTextBox1.Text = listView2.SelectedItems[0].SubItems[2].Text;
                    richTextBox1.SelectionStart = 0;
                    richTextBox1.SelectAll();
                    richTextBox1.SelectionBackColor = Color.White;
                }
                else    MessageBox.Show("일치하는 단어를 찾을 수 없습니다.");
                colorMatchWords(); //일치하는 단어 배경색 변경
            }
        }

        private void colorMatchWords()
        {
            int index = 0;
            while (index < richTextBox1.Text.LastIndexOf(txtKeyword.Text))
            {
                richTextBox1.Find(txtKeyword.Text, index, richTextBox1.TextLength, RichTextBoxFinds.None);
                richTextBox1.SelectionBackColor = Color.GreenYellow;
                index = richTextBox1.Text.IndexOf(txtKeyword.Text, index) + 1;
            }

            string[] words = txtKeyword.Text.Split(',');
            foreach (string word in words)
            {
                int startindex = 0;
                while (startindex < richTextBox1.TextLength)
                {
                    int wordstartIndex = richTextBox1.Find(word, startindex, RichTextBoxFinds.None);
                    if (wordstartIndex != -1)
                    {
                        richTextBox1.SelectionStart = wordstartIndex;
                        richTextBox1.SelectionLength = word.Length;
                        richTextBox1.SelectionBackColor = Color.GreenYellow;
                    }
                    else   break;
                    startindex += wordstartIndex + word.Length;
                }
            }
        }

        private void CopySelectedValuesToClipboard(ListView lv, int index)
        {
            var builder = new StringBuilder();
            foreach (ListViewItem item in lv.SelectedItems)
                builder.AppendLine(item.SubItems[index].Text);
            Clipboard.SetText(builder.ToString());
        }

        private void txtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)    btnKeywordSearch_Click(this, new EventArgs()); //txtKeyword 포커스 중 Enter키 입력 시 btnKeywordSearch 이벤트 (검색 버튼 클릭 이벤트) 실행
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender != listView1) return;
            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedValuesToClipboard(listView1, 1);
        }

        private void listView2_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender != listView2) return;
            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedValuesToClipboard(listView2, 2);
        }
    }
}
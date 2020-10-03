using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CurePlease.App
{
    using static CurePleaseForm;

    public partial class PartyBuffsForm : Form
    {
        private readonly CurePleaseForm _curePleaseForm;

        public class BuffList
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private readonly List<BuffList> _buffNameMap = new List<BuffList>();

        public PartyBuffsForm(CurePleaseForm curePleaseForm)
        {
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();

            _curePleaseForm = curePleaseForm;

            if (_curePleaseForm.setinstance2.Enabled)
            {
                MessageBox.Show("No character was selected as the power leveler, this can not be opened yet.");
                return;
            }

            _buffNameMap = XElement.Load("Resources/Buffs.xml").Elements("o").Select(element => new {
                ID = int.Parse(element.Attribute("id")?.Value ?? "-1"),
                Name = element.Attribute("en")?.Value
            })
                .Where(buff => buff.ID != -1 && buff.Name != null)
                .Select(buff => new BuffList()
                {
                    ID = buff.ID,
                    Name = buff.Name
                }).ToList();
        }

        private void update_effects_Tick(object sender, EventArgs e)
        {
            ailment_list.Text = "";

            // Search through current active party buffs
            foreach (var playerStatusEffects in _curePleaseForm.PartyMemberStatusEffects)
            {
                // First add Character name and a Line Break.
                ailment_list.AppendText(playerStatusEffects.Key.ToUpper() + "\n");

                ailment_list.AppendText(string.Join(
                    ", ",
                    playerStatusEffects.Value.Select(buffId => _buffNameMap.Find(match => match.ID == buffId) + $"({buffId})")
                ));

                ailment_list.AppendText("\n\n");
            }
        }
    }
}

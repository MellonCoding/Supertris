using System.Windows.Forms;
using System.Xml.Linq;
using Supertris.Classes;

namespace Supertris.Forms
{
    partial class SelectionForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnPlayerPlayer = new CustomButton();
            btnPlayerBot = new CustomButton();
            btnBotBot = new CustomButton();
            BtnApriTraining = new CustomButton();
            BtnTipoBot = new CustomButton();
            lblTitolo = new Label();
            SuspendLayout();

            // ── lblTitolo ─────────────────────────────────────────
            lblTitolo.AutoSize = true;
            lblTitolo.Font = new Font("Cascadia Code SemiBold", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitolo.Location = new Point(50, 50);
            lblTitolo.Name = "lblTitolo";
            lblTitolo.TabIndex = 1;
            lblTitolo.Text = "Supertris";

            // ── btnPlayerPlayer ───────────────────────────────────
            btnPlayerPlayer.Location = new Point(50, 150);
            btnPlayerPlayer.Name = "btnPlayerPlayer";
            btnPlayerPlayer.Size = new Size(300, 50);
            btnPlayerPlayer.TabIndex = 0;
            btnPlayerPlayer.Text = "Player vs Player";
            btnPlayerPlayer.Click += btnPlayerPlayer_Click;

            // ── btnPlayerBot ──────────────────────────────────────
            btnPlayerBot.Location = new Point(50, 210);
            btnPlayerBot.Name = "btnPlayerBot";
            btnPlayerBot.Size = new Size(300, 50);
            btnPlayerBot.TabIndex = 2;
            btnPlayerBot.Text = "Player vs Bot";
            btnPlayerBot.Click += btnPlayerBot_Click;

            // ── btnBotBot ─────────────────────────────────────────
            btnBotBot.Location = new Point(50, 270);
            btnBotBot.Name = "btnBotBot";
            btnBotBot.Size = new Size(300, 50);
            btnBotBot.TabIndex = 3;
            btnBotBot.Text = "Bot vs Bot";
            btnBotBot.Click += btnBotBot_Click;

            // ── BtnApriTraining ───────────────────────────────────
            BtnApriTraining.Location = new Point(50, 330);
            BtnApriTraining.Name = "BtnApriTraining";
            BtnApriTraining.Size = new Size(300, 50);
            BtnApriTraining.TabIndex = 4;
            BtnApriTraining.Text = "Allenamento";
            BtnApriTraining.Click += BtnApriTraining_Click;

            // ── BtnTipoBot ────────────────────────────────────────
            BtnTipoBot.Location = new Point(50, 390);
            BtnTipoBot.Name = "BtnTipoBot";
            BtnTipoBot.Size = new Size(300, 50);
            BtnTipoBot.TabIndex = 5;
            BtnTipoBot.Text = "Bot: Albero pesato";
            BtnTipoBot.Click += btnTipoBot_Click;

            // ── Form ──────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 500);
            Controls.Add(BtnTipoBot);
            Controls.Add(BtnApriTraining);
            Controls.Add(btnBotBot);
            Controls.Add(btnPlayerBot);
            Controls.Add(lblTitolo);
            Controls.Add(btnPlayerPlayer);
            Name = "SelectionForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Supertris";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomButton btnPlayerPlayer;
        private CustomButton btnPlayerBot;
        private CustomButton btnBotBot;
        private CustomButton BtnApriTraining;
        private CustomButton BtnTipoBot;
        private Label lblTitolo;
    }
}
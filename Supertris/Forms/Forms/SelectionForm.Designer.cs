namespace SupertrisMeloniSquizzato
{
    partial class SelectionForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnPlayerPlayer = new Button();
            lblTitolo = new Label();
            btnPlayerBot = new Button();
            btnBotBot = new Button();
            BtnApriTraining = new Button();
            BtnTipoBot = new Button();
            SuspendLayout();
            // 
            // lblTitolo
            // 
            lblTitolo.AutoSize = true;
            lblTitolo.Font = new Font("Cascadia Code SemiBold", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitolo.Location = new Point(50, 50);
            lblTitolo.Name = "lblTitolo";
            lblTitolo.Size = new Size(279, 63);
            lblTitolo.TabIndex = 1;
            lblTitolo.Text = "Supertris";
            // 
            // btnPlayerPlayer
            // 
            btnPlayerPlayer.Font = new Font("Cascadia Code SemiBold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            btnPlayerPlayer.Location = new Point(50, 150);
            btnPlayerPlayer.Name = "btnPlayerPlayer";
            btnPlayerPlayer.Size = new Size(300, 50);
            btnPlayerPlayer.TabIndex = 0;
            btnPlayerPlayer.Text = "Player vs Player";
            btnPlayerPlayer.UseVisualStyleBackColor = true;
            btnPlayerPlayer.Click += btnPlayerPlayer_Click;
            // 
            // btnPlayerBot
            // 
            btnPlayerBot.Font = new Font("Cascadia Code SemiBold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            btnPlayerBot.Location = new Point(50, 210);
            btnPlayerBot.Name = "btnPlayerBot";
            btnPlayerBot.Size = new Size(300, 50);
            btnPlayerBot.TabIndex = 2;
            btnPlayerBot.Text = "Player vs Bot";
            btnPlayerBot.UseVisualStyleBackColor = true;
            btnPlayerBot.Click += btnPlayerBot_Click;
            // 
            // btnBotBot
            // 
            btnBotBot.Font = new Font("Cascadia Code SemiBold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            btnBotBot.Location = new Point(50, 270);
            btnBotBot.Name = "btnBotBot";
            btnBotBot.Size = new Size(300, 50);
            btnBotBot.TabIndex = 3;
            btnBotBot.Text = "Bot vs Bot";
            btnBotBot.UseVisualStyleBackColor = true;
            btnBotBot.Click += btnBotBot_Click;
            // 
            // BtnApriTraining
            // 
            BtnApriTraining.Font = new Font("Cascadia Code SemiBold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            BtnApriTraining.Location = new Point(50, 330);
            BtnApriTraining.Name = "BtnApriTraining";
            BtnApriTraining.Size = new Size(300, 50);
            BtnApriTraining.TabIndex = 4;
            BtnApriTraining.Text = "Allenamento";
            BtnApriTraining.UseVisualStyleBackColor = true;
            BtnApriTraining.Click += BtnApriTraining_Click;
            //  
            // BtnTipoBot
            // 
            BtnTipoBot.Font = new Font("Cascadia Code SemiBold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            BtnTipoBot.Location = new Point(50, 390);
            BtnTipoBot.Name = "BtnTipoBot";
            BtnTipoBot.Size = new Size(300, 50);
            BtnTipoBot.TabIndex = 5;
            BtnTipoBot.Text = "Bot: Albero pesato";
            BtnTipoBot.UseVisualStyleBackColor = true;
            BtnTipoBot.Click += btnTipoBot_Click;
            // 
            // SelectionForm
            // 
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
            Text = "SelectionForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnPlayerPlayer;
        private Label lblTitolo;
        private Button btnPlayerBot;
        private Button btnBotBot;
        private Button BtnApriTraining;
        private Button BtnTipoBot;
    }
}
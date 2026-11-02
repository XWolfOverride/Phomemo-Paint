namespace PhomemoPaint
{
    partial class PrinterInfoPanel
    {
        /// <summary>Variable del diseñador necesaria.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Limpiar los recursos que se estén usando.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar el contenido de este
        /// método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._tips = new System.Windows.Forms.ToolTip(this.components);
            this._table = new System.Windows.Forms.TableLayoutPanel();
            this._connectionCaption = new System.Windows.Forms.Label();
            this._connection = new System.Windows.Forms.Label();
            this._addressCaption = new System.Windows.Forms.Label();
            this._address = new System.Windows.Forms.Label();
            this._serialCaption = new System.Windows.Forms.Label();
            this._serial = new System.Windows.Forms.Label();
            this._firmwareCaption = new System.Windows.Forms.Label();
            this._firmware = new System.Windows.Forms.Label();
            this._batteryCaption = new System.Windows.Forms.Label();
            this._battery = new System.Windows.Forms.Label();
            this._paperCaption = new System.Windows.Forms.Label();
            this._paper = new System.Windows.Forms.Label();
            this._coverCaption = new System.Windows.Forms.Label();
            this._cover = new System.Windows.Forms.Label();
            this._updatedCaption = new System.Windows.Forms.Label();
            this._updated = new System.Windows.Forms.Label();
            this._refresh = new PhomemoPaint.IconButton();
            this._table.SuspendLayout();
            this.SuspendLayout();
            //
            // _table
            //
            this._table.AutoSize = true;
            this._table.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._table.ColumnCount = 2;
            this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._table.Controls.Add(this._connectionCaption, 0, 0);
            this._table.Controls.Add(this._connection, 1, 0);
            this._table.Controls.Add(this._addressCaption, 0, 1);
            this._table.Controls.Add(this._address, 1, 1);
            this._table.Controls.Add(this._serialCaption, 0, 2);
            this._table.Controls.Add(this._serial, 1, 2);
            this._table.Controls.Add(this._firmwareCaption, 0, 3);
            this._table.Controls.Add(this._firmware, 1, 3);
            this._table.Controls.Add(this._batteryCaption, 0, 4);
            this._table.Controls.Add(this._battery, 1, 4);
            this._table.Controls.Add(this._paperCaption, 0, 5);
            this._table.Controls.Add(this._paper, 1, 5);
            this._table.Controls.Add(this._coverCaption, 0, 6);
            this._table.Controls.Add(this._cover, 1, 6);
            this._table.Controls.Add(this._updatedCaption, 0, 7);
            this._table.Controls.Add(this._updated, 1, 7);
            this._table.Controls.Add(this._refresh, 0, 8);
            this._table.Location = new System.Drawing.Point(0, 0);
            this._table.Margin = new System.Windows.Forms.Padding(0);
            this._table.Name = "_table";
            this._table.RowCount = 9;
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._table.Size = new System.Drawing.Size(240, 230);
            this._table.TabIndex = 0;
            this._table.SetColumnSpan(this._refresh, 2);
            //
            // _connectionCaption
            //
            this._connectionCaption.AutoSize = true;
            this._connectionCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._connectionCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._connectionCaption.Name = "_connectionCaption";
            this._connectionCaption.Text = "Conexión";
            //
            // _connection
            //
            this._connection.AutoSize = true;
            this._connection.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._connection.MaximumSize = new System.Drawing.Size(160, 0);
            this._connection.Name = "_connection";
            this._connection.Text = "—";
            //
            // _addressCaption
            //
            this._addressCaption.AutoSize = true;
            this._addressCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._addressCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._addressCaption.Name = "_addressCaption";
            this._addressCaption.Text = "Dirección";
            //
            // _address
            //
            this._address.AutoSize = true;
            this._address.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._address.MaximumSize = new System.Drawing.Size(160, 0);
            this._address.Name = "_address";
            this._address.Text = "—";
            //
            // _serialCaption
            //
            this._serialCaption.AutoSize = true;
            this._serialCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._serialCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._serialCaption.Name = "_serialCaption";
            this._serialCaption.Text = "N.º de serie";
            //
            // _serial
            //
            this._serial.AutoSize = true;
            this._serial.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._serial.MaximumSize = new System.Drawing.Size(160, 0);
            this._serial.Name = "_serial";
            this._serial.Text = "—";
            //
            // _firmwareCaption
            //
            this._firmwareCaption.AutoSize = true;
            this._firmwareCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._firmwareCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._firmwareCaption.Name = "_firmwareCaption";
            this._firmwareCaption.Text = "Firmware";
            //
            // _firmware
            //
            this._firmware.AutoSize = true;
            this._firmware.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._firmware.MaximumSize = new System.Drawing.Size(160, 0);
            this._firmware.Name = "_firmware";
            this._firmware.Text = "—";
            //
            // _batteryCaption
            //
            this._batteryCaption.AutoSize = true;
            this._batteryCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._batteryCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._batteryCaption.Name = "_batteryCaption";
            this._batteryCaption.Text = "Batería";
            //
            // _battery
            //
            this._battery.AutoSize = true;
            this._battery.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._battery.MaximumSize = new System.Drawing.Size(160, 0);
            this._battery.Name = "_battery";
            this._battery.Text = "—";
            //
            // _paperCaption
            //
            this._paperCaption.AutoSize = true;
            this._paperCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._paperCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._paperCaption.Name = "_paperCaption";
            this._paperCaption.Text = "Papel";
            //
            // _paper
            //
            this._paper.AutoSize = true;
            this._paper.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._paper.MaximumSize = new System.Drawing.Size(160, 0);
            this._paper.Name = "_paper";
            this._paper.Text = "—";
            //
            // _coverCaption
            //
            this._coverCaption.AutoSize = true;
            this._coverCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._coverCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._coverCaption.Name = "_coverCaption";
            this._coverCaption.Text = "Tapa";
            //
            // _cover
            //
            this._cover.AutoSize = true;
            this._cover.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._cover.MaximumSize = new System.Drawing.Size(160, 0);
            this._cover.Name = "_cover";
            this._cover.Text = "—";
            //
            // _updatedCaption
            //
            this._updatedCaption.AutoSize = true;
            this._updatedCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this._updatedCaption.Margin = new System.Windows.Forms.Padding(0, 2, 3, 2);
            this._updatedCaption.Name = "_updatedCaption";
            this._updatedCaption.Text = "Leído";
            //
            // _updated
            //
            this._updated.AutoSize = true;
            this._updated.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this._updated.MaximumSize = new System.Drawing.Size(160, 0);
            this._updated.Name = "_updated";
            this._updated.Text = "—";
            //
            // _refresh
            //
            this._refresh.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this._refresh.AutoSize = true;
            this._refresh.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this._refresh.Name = "_refresh";
            this._refresh.Text = "";
            this._refresh.UseVisualStyleBackColor = true;
            this._refresh.Click += new System.EventHandler(this.Refresh_Click);
            this._refresh.IconSize = 10F;
            this._refresh.Size = new System.Drawing.Size(3, 26);
            this._refresh.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // PrinterInfoPanel
            //
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._table);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PrinterInfoPanel";
            this.Size = new System.Drawing.Size(240, 230);
            this._table.ResumeLayout(false);
            this._table.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip _tips;
        private System.Windows.Forms.TableLayoutPanel _table;
        private System.Windows.Forms.Label _connectionCaption;
        private System.Windows.Forms.Label _connection;
        private System.Windows.Forms.Label _addressCaption;
        private System.Windows.Forms.Label _address;
        private System.Windows.Forms.Label _serialCaption;
        private System.Windows.Forms.Label _serial;
        private System.Windows.Forms.Label _firmwareCaption;
        private System.Windows.Forms.Label _firmware;
        private System.Windows.Forms.Label _batteryCaption;
        private System.Windows.Forms.Label _battery;
        private System.Windows.Forms.Label _paperCaption;
        private System.Windows.Forms.Label _paper;
        private System.Windows.Forms.Label _coverCaption;
        private System.Windows.Forms.Label _cover;
        private System.Windows.Forms.Label _updatedCaption;
        private System.Windows.Forms.Label _updated;
        private PhomemoPaint.IconButton _refresh;
    }
}

using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Client.Controls;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConexaoKEPserverEX
{
    public partial class Form1 : Form
    {
        bool v = false;
        public Form1(MonitoredItem tagLedWrite = null)
        {
            InitializeComponent();
            this.tagLedWrite = tagLedWrite;
        }
        private Session m_session;
        private bool m_connectedOnce;
        private Subscription m_subscription;
        private void connectServerCtrl1_ConnectComplete(object sender, EventArgs e)
        {
            try
            {
                m_session = connectServerCtrl1.Session;

                // definição de estado inicial
                if (m_session != null && !m_connectedOnce)
                {
                    m_connectedOnce = true;
                    //conectou
                    CreateSubscriptionAndMonitorItem();
                }


            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(this.Text, exception);
            }
        }
        private void CreateSubscriptionAndMonitorItem()
        {
            try
            {
                if (m_session == null)
                {
                    return;
                }

                if (m_subscription != null)
                {
                    m_session.RemoveSubscription(m_subscription);
                    m_subscription = null;
                }

                m_subscription = new Subscription();
                m_subscription.PublishingEnabled = true;
                m_subscription.PublishingInterval = 1000;
                m_subscription.Priority = 1;
                m_subscription.KeepAliveCount = 10;
                m_subscription.LifetimeCount = 20;
                m_subscription.MaxNotificationsPerPublish = 1000;

                m_session.AddSubscription(m_subscription);
                m_subscription.Create();


               
                
                this.textBox1.Text = "---";
                MonitoredItem monitoredItem2 = new MonitoredItem();
                monitoredItem2.StartNodeId = new NodeId("Channel1.Device1.Tag1", 2);
                monitoredItem2.AttributeId = Attributes.Value;
                monitoredItem2.Notification += MonitoredItem2_Notification;
                m_subscription.AddItem(monitoredItem2);
               

                MonitoredItem monitoredItem3 = new MonitoredItem();
                monitoredItem3.StartNodeId = new NodeId("Channel1.Device1.v1", 2);
                monitoredItem3.AttributeId = Attributes.Value;
                monitoredItem3.Notification += MonitoredItem3_Notification;
                m_subscription.AddItem(monitoredItem3);

                m_subscription.ApplyChanges();
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(this.Text, exception);
            }
        }

        private void MonitoredItem3_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MonitoredItemNotificationEventHandler(MonitoredItem3_Notification), monitoredItem, e);
                return;
            }
            try
            {
                //verifica estados de entrada
                if ((bool)((MonitoredItemNotification)e.NotificationValue).Value.WrappedValue.Value == true)
                {
                    this.pictureBox1.Image = global::ConexaoKEPserverEX.Properties.Resources.BlueButton;
                    label3.Text = "LIGADO";
                    label3.ForeColor = Color.Blue;
                }
                else
                {
                    this.pictureBox1.Image = global::ConexaoKEPserverEX.Properties.Resources.BlackButtonPressed;
                    label3.Text = "DESLIGADO";
                    label3.ForeColor = Color.Black;
                }
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }

        private void MonitoredItem2_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MonitoredItemNotificationEventHandler(MonitoredItem2_Notification), monitoredItem, e);
                return;
            }
            this.textBox1.Text = ((MonitoredItemNotification)e.NotificationValue).Value.WrappedValue.ToString();

        }

        private void MonitoredItem_Notification1(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MonitoredItemNotificationEventHandler(MonitoredItem_Notification1), monitoredItem, e);
                return;
            }

            try
            {
                object[] objs = (object[])monitoredItem.Handle;
                Control control = (Control)objs[0];
                PropertyInfo proInfo = (PropertyInfo)objs[1];
                MonitoredItemNotification datachange = e.NotificationValue as MonitoredItemNotification;

                if (datachange == null)
                {
                    return;
                }
                object v = TypeDescriptor.GetConverter(datachange.Value.WrappedValue.Value.GetType()).ConvertTo(datachange.Value.WrappedValue, proInfo.PropertyType);
                if (proInfo != null) proInfo.SetValue(control, v);
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(this.Text, exception);
            }
        }

        private void MonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.connectServerCtrl1.ServerUrl = "opc.tcp://DESKTOP-K41VSBF:49320";//conexão com servidor.
            string AppName = "Anonimo";//usuario anonimo para teste.
            ApplicationConfiguration config = new ApplicationConfiguration()
            {
                ApplicationName = AppName,
                ApplicationUri = Utils.Format(@"urn:{0}:" + AppName, System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = @"Directory",
                        StorePath = System.Windows.Forms.Application.StartupPath + @"\Cert\TrustedIssuer",
                        SubjectName = "CN=" + AppName + ", DC=" + System.Net.Dns.GetHostName()
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = System.Windows.Forms.Application.StartupPath + @"\Cert\TrustedIssuer"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = System.Windows.Forms.Application.StartupPath + @"\Cert\TrustedIssuer"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = System.Windows.Forms.Application.StartupPath + @"\Cert\RejectedCertificates"
                    },
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = true,
                    RejectSHA1SignedCertificates = false//rejeição de certificados.
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration
                {
                    DeleteOnLoad = true
                },
                DisableHiResClock = false

            };
            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, ee) =>
                { ee.Accept = (ee.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            this.connectServerCtrl1.Configuration = config;
            //conecta com usuario e senha
            this.connectServerCtrl1.UserIdentity = new UserIdentity();
            this.connectServerCtrl1.UseSecurity = true;

            var application = new ApplicationInstance
            {
                ApplicationName = AppName,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config

            };
            //define a mascara de rastreamento e gera a interrupção de logs na saída.
            Opc.Ua.Utils.SetTraceMask(0);//
            application.CheckApplicationInstanceCertificate(true, 2048).GetAwaiter().GetResult();//gera o certificado digital

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.connectServerCtrl1.Disconnect();
        }
        MonitoredItem tagLedWrite;
        private void lbButton1_MouseDown(object sender, MouseEventArgs e)
        {

           v = false;
           //this.WriteTag(this.connectServerCtrl1.Session, "Channel1.Device1.v1", "Boolean",!v);
            this.WriteTag(this.connectServerCtrl1.Session, this.tagLedWrite, !v);

        }
        bool WriteTag(Session m_session, MonitoredItem tag, Object v)
        {
            Opc.Ua.WriteValue valueToWrite = new Opc.Ua.WriteValue();
            valueToWrite.AttributeId = Attributes.Value;
            string sType = "Boolean";
            string tagID = "Channel1.Device1.v1";
            return WriteTag(m_session, tagID, sType, v);
        }

        bool WriteTag(Session m_session, string tag, string sType, object v)
        {
            Opc.Ua.WriteValue valueToWrite = new Opc.Ua.WriteValue();
            valueToWrite.AttributeId = Attributes.Value;
            valueToWrite.NodeId = new NodeId(tag, 2);
            valueToWrite.Value.Value = GetValue(v, sType);
            valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
            valueToWrite.Value.StatusCode = StatusCodes.Good;

            WriteValueCollection lstToWrite = new WriteValueCollection();
            lstToWrite.Add(valueToWrite);

            StatusCodeCollection results = null;
            DiagnosticInfoCollection lstDia = null;
            m_session.Write(null, lstToWrite, out results, out lstDia);
            ClientBase.ValidateResponse(results, lstToWrite);
            if (StatusCode.IsBad(results[0]))
            {
                return false;
            }
            return true;
        }

        private object GetValue(object v, string sType)
        {
            switch (sType)
            {
                case "Boolean":
                    return Convert.ToBoolean(v);
                case "Byte":
                    return Convert.ToByte(v);
                case "SByte":
                    return Convert.ToSByte(v);
                case "UInt16":
                    return Convert.ToUInt16(v);
                case "Int16":
                    return Convert.ToInt16(v);
                case "UInt32":
                    return Convert.ToUInt32(v);
                case "Int32":
                    return Convert.ToInt32(v);
                case "UInt64":
                    return Convert.ToUInt64(v);
                case "Int64":
                    return Convert.ToInt64(v);
                case "Double":
                    return Convert.ToDouble(v);
                case "Float":
                    return Convert.ToDateTime(v);
                case "DateTime":
                    return Convert.ToDateTime(v);
            }
            return v;
        }

        private void lbButton1_MouseUp(object sender, MouseEventArgs e)
        {
           v = true;
            //this.WriteTag(this.connectServerCtrl1.Session, "Channel1.Device1.v1", "Boolean",!v);
            this.WriteTag(this.connectServerCtrl1.Session, this.tagLedWrite, !v);
        }

        
    }
}

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PurchaseReturnReportViewer.aspx.cs" Inherits="MMS.PurchaseReturnReportViewer" MasterPageFile="~/Template.Master"%>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Panel ID="Panel11" runat="server" Width="65%" Height="600px" style="position:relative;">
        <div class="card">
            <div class="card-body">
                <div style="height: 100%; width: 100%; position: relative;">
                    <rsweb:ReportViewer ID="ReportViewer1" runat="server" 
                        Font-Names="Verdana" 
                        AsyncRendering="true" 
                        KeepSessionAlive="true" 
                        SizeToReportContent="true" 
                        Font-Size="8pt" 
                        InteractiveDeviceInfos="(Collection)" 
                        WaitSuccessFont-Names="Verdana" 
                        WaitSuccessFont-Size="14pt" 
                        Width="100%" 
                        Height="100%" ShowToolBar="true"  >
                    </rsweb:ReportViewer>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>

<!--
----------------------------------------------------------------------------
 Autora: Isabel Piñuel González 
 Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
 Universidad Internacional de la Rioja (UNIR)
 Este código se ofrece bajo licencia MIT
------------------------------------------------------------------------------
-->
<%@ Page Language="VB" AutoEventWireup="false" CodeFile="WebTelemed.aspx.vb" Inherits="telemed" validateRequest="false"%>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagPrefix="ACTk" %>
<%@ Register src="WUC_consulta.ascx" tagname="WUC_consulta" tagprefix="uc0" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.0//EN" "http://www.w3.org/TR/html4/strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="x-UA-compatible" content="IE=9"/>
    <link rel="Shortcut Icon" href="~/graficos/teleme.ico"/>
    <title>Telemedicina Diagnóstico remoto</title>
    <style type="text/css">
        .FondoAplicacion
        {
            background-color: Gray;                             
        }
        .FondoTransparente
        {
            background-color: Gray;
        }
        .style_arial_normal
        {
            width: 100%;
            font-style:normal;
            font-family:Arial;
            font-size:larger;
            height: 30px;
        }
        .style4
        {
            width: 250px;
            font: arial;
            font-size:10;
        }
        
       .updateProgress
       {
           border-width: 1px;
           border-style: solid;
           background-color: #FFFF66;
           font-size: 16px;
           position: absolute;
           width: 180px;  /* width of the Modal window */
           height: 65px;  /* height of the Modal window */
        }
        
       .FondoProgress
       {
           background-color: White;
       }
               
    </style>
</head>

<body>
    <form id="frm_telemed" runat="server">
    <table width="798">
        <tr>
        <td align="left">
            <asp:Image ID="logo" runat="server" 
                ImageUrl="~/graficos/logo.jpg" ImageAlign="left">
            </asp:Image>
        </td>
        <td align="center">
            <asp:Label ID="lbl_titulo1" runat="server" Text="TELEMEDICINA" 
                borderStyle="None" 
                ForeColor="#C4171B" Font-Bold="True"
                Font-Names="Arial" Font-Size="Large" Height="30px" Font-Underline="False" 
                ></asp:Label>
            <br />
            <asp:Label ID="lbl_titulo2" runat="server" Text="SISTEMA DE DIAGNÓSTICO REMOTO" 
                borderStyle="None" 
                ForeColor="#C4171B" Font-Bold="True"
                Font-Names="Arial" Font-Size="Large" Height="20px" 
                ></asp:Label>
            <br />
        </td>
        <td align="center">
            <asp:Button id="btn_cerrar" runat="server" Text="Cerrar sesión" Height="21px" Width="100px" TabIndex="90" Font-Bold="True"/>
        </td>
        </tr>
    </table>

    <asp:Login ID="Login1" runat="server"
        style="font-family: Arial, Helvetica, sans-serif; font-size: 14px;"
        DisplayRememberMe="False" 
        TitleText="Proceso de entrada"
        UserName=""
        UserNameLabelText="Usuario:"
        LoginButtonText="Iniciar sesión"
        borderStyle="Outset" FailureText="No está acreditado." TabIndex="1">
        <TextBoxStyle Font-Names="Arial" Font-Size="10pt" Height="18px" Width="80px" />
        <LoginButtonStyle  Height="25px" Width="100px" />
        <TitleTextStyle BackColor="#9966FF" borderStyle="Inset" Font-Italic="True" 
            ForeColor="Black" />
    </asp:Login>
    
    <ACTk:ModalPopupExtender
        ID="mpe_login" runat="server"
        PopupControlID="Login1" 
        TargetControlID="lblOcultoID"
        BackgroundCssClass="FondoAplicacion"
        DropShadow="false"> 
    </ACTk:ModalPopupExtender>
    
    <asp:Label ID="lblOcultoID" runat="server" Text="Label" Style="display: none;" /> 

    <ACTk:ToolkitScriptManager ID="scr_ACTK" runat="server" 
        AsyncPostBackTimeout="0">
       <Scripts>
             <asp:ScriptReference Path="~/jsUpdateProgress.js"/>
       </Scripts>
    </ACTk:ToolkitScriptManager>
    
    <div id="Div1" runat="server">
        <script type="text/javascript" language="javascript">
            var ModalProgress = '<%= ModalProgress.ClientID %>';
        </script>
    </div>
    
    <asp:UpdatePanel ID="upd_panelespera" runat="server">
    <ContentTemplate>

    <div style="width:800px; height:600;">
        <table width="800" cellpadding="0" cellspacing="0" align="center"> 
            <tr valign="top">
                <td>
                    <uc0:WUC_consulta ID="WUC_consulta" runat="server" />
                </td>
            </tr>
        </table>
    </div>

    </ContentTemplate>
    </asp:UpdatePanel>      

    <ACTk:ModalPopupExtender
       ID="ModalProgress" runat="server"
       TargetControlID="panelUpdateProgress"
       PopupControlID="panelUpdateProgress"
       BackgroundCssClass="FondoProgress">
    </ACTk:ModalPopupExtender>
    <asp:Panel ID="panelUpdateProgress" runat="server" CssClass="updateProgress" > 
        <asp:UpdateProgress ID="UpdateProg1" DisplayAfter="0" runat="server" 
            AssociatedUpdatePanelID="upd_panelespera">
           <ProgressTemplate>
             <div style="position: relative; top: 30%; text-align: center;">
               <img src="graficos/indicator.gif" style="vertical-align:middle" alt="Procesando"/>
               <marquee> procesando datos espere... </marquee>
             </div>
           </ProgressTemplate>
        </asp:UpdateProgress>
    </asp:Panel>     
    <asp:Button ID="btn_mensaje_progress" runat="server" Height="1px" Width="1px" style="display: none;" />

    </form>
</body>
</html>

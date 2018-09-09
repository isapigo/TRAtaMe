<!--
------------------------------------------------------------------------------
Autora: Isabel Piñuel González 
Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
Universidad Internacional de la Rioja (UNIR)
Este código se ofrece bajo licencia MIT
------------------------------------------------------------------------------
-->
<%@ Control Language="VB" AutoEventWireup="false" CodeFile="WUC_consulta.ascx.vb" Inherits="WUC_consulta" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagPrefix="ACTk" %>
    <style type="text/css">
        .style_arial_normal
        {
            width: 100%;
            font-style: normal;
            font-family:Arial;
            font-size: small;
            height: auto;
            color: White; 
        }
        .style4
        {
            width: 250px;
            font: arial;
            font-size:small;
        }
        
        .ButtonCell
        {
            vertical-align:bottom;
        }        
        .style5
        {
            width: 625px;
        }
    </style>
    
    <div class="style_arial_normal">
        <table align="center" width="500" border="5" bgcolor="#9966FF">
            <tr class="style_arial_normal">
                <td align="right" >
                    <asp:Label ID="lbl_doctor" runat="server" Text="Doctor/a:" Font-Bold="True"></asp:Label>
                </td>
                <td align="left">
                    <asp:TextBox ID="txt_doctor" runat="server" Width="410" BackColor="Silver" ReadOnly="True"></asp:TextBox>
                </td>
            </tr>
        </table>
        <br/>
        <table align="center" width="735" bgcolor="#9966FF" border="5" >
            <tr class="style_arial_normal">
                <td align="center" width="235">
                    <asp:Label ID="lbl_nhc" runat="server" Text="NHC:" Font-Bold="True"></asp:Label>
                    <asp:TextBox ID="txt_nhc" runat="server" Width="70px" MaxLength="7"></asp:TextBox> 
                    <asp:Button ID="btn_nhc" runat="server" Text="Poner" Font-Bold="True" Width="70px" ToolTip="Si teclea el NHC puede ponerlo al paciente seleccionado." Visible="False"/>               
                </td>
                <td align="left" width="360">
                    <asp:Label ID="lbl_gfh" runat="server" Text="Serv:" Font-Bold="True"></asp:Label>
                    <asp:DropDownList ID="cmb_gfh" runat="server" Width="300px"></asp:DropDownList>
                </td>
                <td align="left" width="150">
                    <asp:Label ID="lbl_fecha" runat="server" Text="Desde:" Font-Bold="True"></asp:Label>
                    <asp:TextBox ID="txt_fecha" runat="server" Width="70px"></asp:TextBox> 
                </td>
                <td align="center" width="50">
                    &nbsp;<asp:ImageButton ID="btn_pacientes" height="20" width="20" runat="server" ToolTip="Seleccionar paciente visitado" ImageUrl="~/graficos/S.jpg" borderStyle="Outset" borderWidth="2px" ForeColor="#3366FF" />
                </td>
           </tr> 
        </table>
        <table align="center" width="798" border="3" bgcolor="#9966FF">
            <tr class="style_arial_normal">
                <td align="left" width="150">
                    <asp:Label ID="lbl_episodio" runat="server" Text="Episodio:" Font-Bold="True"></asp:Label>
                    <asp:TextBox ID="txt_episodio" runat="server" Width="50px" AutoPostBack="True" BackColor="Silver" ReadOnly="True" Font-Bold="True"></asp:TextBox>
                </td>
                <td align="left" width="500">
                    <asp:Label ID="lbl_nombre" runat="server" Text="Nombre:" Font-Bold="True"></asp:Label>
                    <asp:TextBox ID="txt_nombre" runat="server" Width="400px" BackColor="Silver" ReadOnly="True" Font-Bold="True"></asp:TextBox>
                </td>
                <td align="left" width="148">
                    <asp:Label ID="lbl_edad" runat="server" Text="Edad:" Font-Bold="True"></asp:Label>
                    <asp:TextBox ID="txt_edad" runat="server" Width="40px" BackColor="Silver" ReadOnly="True" Font-Bold="True"></asp:TextBox>
                </td>
            </tr> 
        </table>
        <table align="center" width="798" bgcolor="#9966FF" border="3">
            <tr align="center" class="style_arial_normal">
                <td align="center">
                    <asp:Label ID="lbl_pregunta" runat="server" Text="Pregunta:" ></asp:Label>
                    <br/>
                    <asp:TextBox ID="txt_pregunta" runat="server" Width="380px" Height="100px" 
                        TextMode="MultiLine" ReadOnly="True"></asp:TextBox>
                    <br/>
                    <asp:Button ID="btn_pregunta" runat="server" Text="Abre ventana para añadir texto" Font-Bold="True" height="20px" width="220px" Enabled="False"/>
                </td>
                <td >
                    <asp:Label ID="lbl_respuesta" runat="server" Text="Respuesta:" ></asp:Label>
                    <br/>                    
                    <asp:TextBox ID="txt_respuesta" runat="server" Width="380px" Height="100px" 
                        TextMode="MultiLine" ReadOnly="True"></asp:TextBox>
                    <br/>                    
                    <asp:Button ID="btn_respuesta" runat="server" Text="Abre ventana para añadir texto" Font-Bold="True" height="20px" width="220px" Enabled="False"/>
                </td>
            </tr>
        </table>
        <table align="center" width="798" bgcolor="#9966FF" border="3">
            <tr align="center" class="style_arial_normal">
                <td >
                
                    <asp:Label ID="lbl_audios" runat="server" Text="Audio" Font-Bold="True"></asp:Label>
                    <br/>
                    <asp:ImageButton id="img_amenos1" height="20" width="20" runat="server" 
                        ToolTip="Anterior audio" ImageUrl="~/graficos/menos1.jpg" 
                        borderStyle="Outset" borderWidth="2px" Enabled="False" />
                    <asp:ImageButton id="btn_play" runat="server"
                        ToolTip="Escuchar audio" ImageUrl="~/graficos/play.png" 
                        borderStyle="Outset" borderWidth="2px"
                        Height="20px" Width="35px" Enabled="False"/>
                    <asp:ImageButton id="img_amas1" height="20" width="20" runat="server" 
                        ToolTip="Siguiente audio" ImageUrl="~/graficos/mas1.jpg" 
                        borderStyle="Outset" borderWidth="2px" Enabled="False" />
                       <br/>
                       <br/>
                    <asp:Panel ID="pan_rdb" runat="server" Direction="LeftToRight">
                       <asp:RadioButton ID="rdb_foto" runat="server" Text="Foto" Width="100" 
                            OnClick="Changerdb(this.value)" style="height: 23px" />
                       <br/>
                       <asp:RadioButton ID="rdb_video" runat="server" Text="Vídeo" Width="100" OnClick="Changerdb(this.value)"/>
                       <br/>
                       <asp:Button ID="btn_clic_rdb" runat="server" Height="1px" Width="1px" style="display: none;"/>
                    </asp:Panel>
                    &nbsp;<asp:ImageButton ID="img_menos1" height="20" width="20" runat="server" ToolTip="Anterior multimedia" ImageUrl="~/graficos/menos1.jpg" borderStyle="Outset" borderWidth="2px" ForeColor="#3366FF" />
                    <asp:Label ID="lbl_nada" runat="server" Text="- +" Font-Bold="True" 
                        Font-Size="Large" ForeColor="White"></asp:Label>
                    &nbsp;<asp:ImageButton ID="img_mas1" height="20" width="20" runat="server" ToolTip="Siguiente multimedia" ImageUrl="~/graficos/mas1.jpg" borderStyle="Outset" borderWidth="2px" ForeColor="#3366FF" />
                    <br/>                       
                    <br/>
                    <br/>
                    <asp:Label ID="lbl_audio" runat="server" Text="0 Audios"></asp:Label>
                    <br/>                   
                    <asp:Label ID="lbl_foto" runat="server" Text="0 Fotos"></asp:Label>
                    <br/>                   
                    <asp:Label ID="lbl_video" runat="server" Text="0 Vídeos"></asp:Label>
                    <br/>
                    <br/>
                    <asp:Label ID="lbl_litfic" runat="server" Text="Fich: "></asp:Label>
                    <asp:Label ID="lbl_id_media" runat="server" Text="0"></asp:Label>
                </td>
                <td>
                                           <br/>

                    <br/>
                    <video id="vid_multimed" runat="server"
                    type="video/mp4"
                    width="0" 
                    height="0"
                    src=""                    
                    frameborder="0">
                    <object>
                    </object>
                    </video> 
                    <iframe id="ifm_multimed" runat="server"
                    width="660" 
                    height="350"
                    src=""
                    frameborder="0">
                    </iframe>
                    <audio id="aud_play" src="" runat="server"></audio>
                </td>
            </tr>
        </table>
        
        <br/>
        <table align="center"  class="style4">
            <tr>
                <td align="center" height="20" width="140" >
                    <asp:Button ID="btn_limpiar" runat="server" Text="Limpiar Datos" Font-Bold="True" 
                     Width="140px" ToolTip="Limpia los datos de la pantalla"/>
                </td>
                <td height="20" width="20">
                </td>
                <td align="center" height="20" width="140">
                    <asp:Button ID="btn_borra" runat="server" Text="Borrar Episodio" Font-Bold="True" 
                     Width="140px" Enabled="False" 
                        ToolTip="Elimina toda la información del episodio" Visible="False"/>
                </td>
                <td height="20" width="20">
                </td>
           </tr>
           <ACTk:ConfirmButtonExtender 
           ID="con_elimina" runat="server"
           ConfirmText = "Confirme que desea anular la situacion."
           TargetControlID="btn_borra">
           </ACTk:ConfirmButtonExtender>
           </table>
    <asp:Label ID="lbl_mpe_ayuda" runat="server" Text="Label" Style="display: none;" />
    <ACTk:ModalPopupExtender
        ID="mpe_ayuda" runat="server" 
        BackgroundCssClass="FondoTransparente"
        DropShadow="false"
        CancelControlID="btn_ayuda"
        PopupControlID="pan_ayuda"
        TargetControlID="lbl_mpe_ayuda"
    >   
    </ACTk:ModalPopupExtender>
    <asp:Panel ID="pan_ayuda" runat="server" Width="250" Height="40" BackColor="#FFFFCC" borderStyle="Outset" borderWidth="2px" Wrap="False">
    <div id="div_pan" style="width: auto; height: auto;" align="center" class="style_arial_normal">
        <br/>
        <asp:Image ID="img_ayuda" runat="server" ImageUrl="~/graficos/i.jpg" />
        <br/>
        <asp:Label ID="lbl_ayuda_cab" runat="server" Text="cabecera de la ayuda" 
            Height="20px" Width="250px" Font-Bold="True" Font-Overline="True" 
            Font-Underline="True" ForeColor="#0102FD"></asp:Label>
        <br/>
        <asp:TextBox ID="txt_ayuda" runat="server" Text="por aqui muestro la ayuda" 
            Height="40px" Width="250px" BackColor="#FFFFCC" borderColor="White" 
            borderStyle="None" Font-Bold="True" Font-Names="Arial" 
            TextMode="MultiLine" borderWidth="4px"></asp:TextBox>
        <br/>
        <table>
           <tr>
              <td align="center">
                 <asp:Button ID="btn_ayuda" runat="server" Text="Cerrar" Font-Bold="True"/>
              </td>
           </tr>
        </table>
    </div>  
    </asp:Panel>

    <asp:Label ID="lbl_mpe_aviso" runat="server" Text="Label" Style="display: none;" />
    <ACTk:ModalPopupExtender
        ID="mpe_aviso" runat="server" 
        BackgroundCssClass="FondoTransparente"
        DropShadow="false"
        CancelControlID="btn_aviso"
        PopupControlID="pan_aviso"
        TargetControlID="lbl_mpe_aviso"
    >   
    </ACTk:ModalPopupExtender>
    
    <asp:Panel ID="pan_aviso" runat="server" Width="455" Height="150" 
        BackColor="#FFFFCC" borderStyle="Outset" borderWidth="2px">
    <div style="width: 455px;" align="center" class="style_arial_normal">
        <br/>
        <asp:Label ID="lbl_aviso_1" runat="server" Text="por aqui muestro el aviso" 
            Height="20px" Width="450px" ForeColor="#0102FD"></asp:Label>
        <asp:Label ID="lbl_aviso_2" runat="server" Text="por aqui muestro el aviso" 
            Height="20px" Width="450px" ForeColor="#0102FD"></asp:Label>
        <asp:Label ID="lbl_aviso_3" runat="server" Text="por aqui muestro el aviso" 
            Height="20px" Width="450px" ForeColor="#0102FD"></asp:Label>
        <table>
           <tr>
              <td align="center">
                 <asp:Button ID="btn_aviso" runat="server" Text="Cerrar" Font-Bold="True"/>
              </td>
           </tr>
        </table>
    </div>  
    </asp:Panel>
    <asp:HiddenField ID="hid_entra" runat="server" Value="N" />
    <asp:HiddenField ID="hid_codemp" runat="server" Value="0" />
    <asp:HiddenField ID="hid_indserv" runat="server" Value="" />
    <asp:HiddenField ID="hid_episodio" runat="server" Value="" />
    <asp:HiddenField ID="hid_mostrar" runat="server" Value="N" />
    <asp:HiddenField ID="hid_mostrar_aviso" runat="server" Value="N" />
    <asp:HiddenField ID="hid_mostrar_ayuda" runat="server" Value="N" />
    <asp:HiddenField ID="hid_mostrar_textos" runat="server" Value="N" />
    <asp:HiddenField ID="hid_es_nhc" runat="server" Value="N" />

    </div>

    <asp:Label ID="lbl_mpe_pacientes" runat="server" Text="Label" Style="display: none;" />
    <ACTk:ModalPopupExtender
        ID="mpe_pacientes" runat="server" 
        BackgroundCssClass="FondoSolicitudes"
        DropShadow="false"
        CancelControlID="btn_cerrar_pacientes"
        PopupControlID="pan_pacientes"
        TargetControlID="lbl_mpe_pacientes"
    >   
    </ACTk:ModalPopupExtender>
    <asp:Panel ID="pan_pacientes" runat="server" Width="500" Height="200" BackColor="#FFFFCC" borderStyle="Outset" borderWidth="2px" Wrap="False">
    <div id="div1" style="width: auto; height: auto;" align="center" class="style_arial_normal">
        <br/>
        <asp:Image ID="img_pacientes" runat="server" ImageUrl="~/graficos/S.jpg" />
        <br/>
        <asp:Label ID="lbl_pacientes" runat="server" Text="Pacientes visitados" 
            Height="20px" Width="450px" Font-Bold="True" Font-Overline="True" 
            Font-Underline="True" ForeColor="#0102FD"></asp:Label>
        <br/>
        <br/>
        <table>
           <tr>
              <td align="center">              
                  <asp:DropDownList ID="cmb_pacientes" runat="server" Width="450px" OnChange="ChangePaciente(this.value)">
                  </asp:DropDownList>
              </td>
          </tr>
        </table>
        <br/>
        <br/>
        <table>
           <tr>
              <td align="center">
                 <asp:Button ID="btn_cerrar_pacientes" runat="server" Text="Cerrar" Font-Bold="True"/>
              </td>
              <td>
              </td>
              <td align="center">
                 <asp:Button ID="btn_ver_pacientes" runat="server" Text="Seleccionar" Font-Bold="True"/>
              </td>
           </tr>
        </table>
    </div>
    </asp:Panel>
    <asp:Label ID="lbl_pon_texto" runat="server" Text="" Style="display: none;" />
    <ACTk:ModalPopupExtender
        ID="mpe_pon_textos" runat="server" 
        BackgroundCssClass="FondoSolicitudes"
        DropShadow="false"
        CancelControlID="btn_pon_textos_cerrar"
        PopupControlID="pan_pon_textos"
        TargetControlID="lbl_pon_texto"
    >   
    </ACTk:ModalPopupExtender>
    <asp:Panel ID="pan_pon_textos" runat="server" Width="500" Height="400" BackColor="#FFFFCC" borderStyle="Outset" borderWidth="2px" Wrap="False">
    <div id="div2" style="width: auto; height: auto;" align="center" class="style_arial_normal">
        <br/>
        <asp:Image ID="img_texto_libre" runat="server" ImageUrl="~/graficos/i.jpg" />
        <br/>
        <asp:Label ID="lbl_texto_libre" runat="server" Text="Texto libre" 
            Height="20px" Width="450px" Font-Bold="True" Font-Overline="True" 
            Font-Underline="True" ForeColor="#0102FD"></asp:Label>
        <br/>
        <br/>
        <table>
           <tr>
              <td align="center">              
                    <asp:TextBox ID="txt_pon_textos" runat="server" Width="380px" Height="150px" 
                        TextMode="MultiLine"></asp:TextBox>
              </td>
          </tr>
        </table>
        <asp:Label ID="lbl_pon_textos_gfh" runat="server" Text="Servicios a los que se realiza la pregunta" 
               Height="20px" Width="450px" Font-Bold="True" ForeColor="#0102FD"></asp:Label>
        <br/>
        <table>
           <tr>
              <td align="left">
                  <asp:CheckBoxList ID="cmb_gfh_pon_textos" runat="server" Width="200px" 
                      Font-Bold="True" ForeColor="#0102FD" BorderStyle="Solid" RepeatColumns="2">
                  </asp:CheckBoxList>
              </td>
              </tr>
        </table>
        <table>
              <tr>
              <td align="center">
                 <asp:Button ID="btn_pon_textos_cerrar" runat="server" Text="Cerrar" Font-Bold="True"/>
              </td>
              <td>
              </td>
              <td align="center">
                 <asp:Button ID="btn_pon_textos_aceptar" runat="server" Text="Aceptar" Font-Bold="True"/>
              </td>
           </tr>
        </table>
        <br/>
    </div>
    </asp:Panel>

    <ACTk:ModalPopupExtender
        ID="mpe_es_nhc" runat="server" 
        BackgroundCssClass="FondoSolicitudes"
        DropShadow="false"
        CancelControlID="btn_es_nhc_no"
        PopupControlID="pan_es_nhc"
        TargetControlID="lbl_es_nhc"
    >   
    </ACTk:ModalPopupExtender>

    <asp:Panel ID="pan_es_nhc" runat="server" Width="500" Height="300" BackColor="#FFFFCC" borderStyle="Outset" borderWidth="2px" Wrap="False">
    <div id="div3" style="width: auto; height: auto;" align="center" class="style_arial_normal">
        <br/>
        <asp:Image ID="img_es_nhc" runat="server" ImageUrl="~/graficos/S.jpg" />
        <br/>
        <asp:Label ID="lbl_es_nhc" runat="server" Text="Comprobar NHC" 
            Height="20px" Width="450px" Font-Bold="True" Font-Overline="True" 
            Font-Underline="True" ForeColor="#0102FD"></asp:Label>
        <br/>
        <br/>
        <table>
           <tr>
              <td align="center">              
                    <asp:TextBox ID="txt_es_nhc" runat="server" Width="380px" Height="150px" 
                        TextMode="MultiLine"></asp:TextBox>
              </td>
          </tr>
        </table>
        <br/>
        <table>
           <tr>
              <td align="center">
                 <asp:Button ID="btn_es_nhc_si" runat="server" Text="Aceptar" Font-Bold="True"/>
              </td>
              <td>
              </td>
              <td align="center">
                 <asp:Button ID="btn_es_nhc_no" runat="server" Text="Cancelar" Font-Bold="True"/>
              </td>
           </tr>
        </table>
        <br/>
    </div>
    </asp:Panel>

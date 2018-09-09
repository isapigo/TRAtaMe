' ------------------------------------------------------------------------------
' Autora: Isabel Piñuel González 
' Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
' Universidad Internacional de la Rioja (UNIR)
' Este código se ofrece bajo licencia MIT
' ------------------------------------------------------------------------------
Partial Class telemed
    Inherits System.Web.UI.Page
    ' cambie DOCTYPE... antes: <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
    'Estructura de datos del usuario
    Private datos_usuario As New WStelemed.cls_usuario
    Private datos_paciente As New WStelemed.cls_paciente
    Private datos_informacion As New WStelemed.cls_informacion

    Protected Sub Login1_Authenticate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs) Handles Login1.Authenticate
        Dim Password As String
        Dim lit_error As String
        Dim datos_usuario As New WStelemed.cls_usuario
        Dim WS As New WStelemed.telediagnosticoWS

        Password = WS.mira_nulo(Login1.Password)

        lit_error = "No está acreditado."

        datos_usuario.usuario = WS.mira_nulo(UCase(Trim(Login1.UserName)))
        datos_usuario.nombre = ""
        datos_usuario.permiso = 0
        datos_usuario.codser = ""
        datos_usuario.origen = ""
        datos_usuario.puesto = ""
        datos_usuario.unidad = ""

        If funciones.valida_entrada(datos_usuario.usuario, Password, lit_error, datos_usuario) Then

            Session.Add("datos_usuario", datos_usuario)

            e.Authenticated = True
        Else
            If IsNumeric(datos_usuario.usuario) Then datos_usuario.codemp = datos_usuario.usuario
            datos_usuario.dni = datos_usuario.usuario

            e.Authenticated = False
            datos_usuario.codemp = 0
        End If

        Login1.FailureText = lit_error

    End Sub

    Protected Sub Login1_LoggedIn(ByVal sender As Object, ByVal e As System.EventArgs) Handles Login1.LoggedIn
        Server.Transfer("~/WebTelemed.aspx")
    End Sub

    Protected Sub btn_cerrar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_cerrar.Click
        datos_usuario = Session("datos_usuario")
        funciones.cierro(datos_usuario, Session)
        Response.Redirect("WebTelemed.aspx")
    End Sub

    Protected Sub btn_mensaje_progress_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_mensaje_progress.Click

        Dim WUC_consulta As System.Web.UI.UserControl = Me.FindControl("WUC_consulta")
        Dim hid_mostrar As System.Web.UI.WebControls.HiddenField = WUC_consulta.FindControl("hid_mostrar")
        Dim hid_mostrar_aviso As System.Web.UI.WebControls.HiddenField = WUC_consulta.FindControl("hid_mostrar_aviso")
        Dim hid_mostrar_ayuda As System.Web.UI.WebControls.HiddenField = WUC_consulta.FindControl("hid_mostrar_ayuda")
        Dim hid_mostrar_textos As System.Web.UI.WebControls.HiddenField = WUC_consulta.FindControl("hid_mostrar_textos")
        Dim hid_es_nhc As System.Web.UI.WebControls.HiddenField = WUC_consulta.FindControl("hid_es_nhc")

        If hid_mostrar.Value = "S" Then
            Dim mpe_pacientes As AjaxControlToolkit.ModalPopupExtender = WUC_consulta.FindControl("mpe_pacientes")
            mpe_pacientes.Show()
            hid_mostrar.Value = "N"
        Else
            If hid_mostrar_aviso.Value = "S" Then
                Dim mpe_aviso As AjaxControlToolkit.ModalPopupExtender = WUC_consulta.FindControl("mpe_aviso")
                mpe_aviso.Show()
                hid_mostrar_aviso.Value = "N"
            End If
        End If

        If hid_mostrar_ayuda.Value = "S" Then
            Dim mpe_ayuda As AjaxControlToolkit.ModalPopupExtender = WUC_consulta.FindControl("mpe_ayuda")
            mpe_ayuda.Show()
            hid_mostrar_ayuda.Value = "N"
        End If

        If hid_mostrar_textos.Value = "S" Then
            Dim mpe_pon_textos As AjaxControlToolkit.ModalPopupExtender = WUC_consulta.FindControl("mpe_pon_textos")
            mpe_pon_textos.Show()
            hid_mostrar_textos.Value = "N"
        End If

        If hid_es_nhc.Value = "S" Then
            Dim mpe_es_nhc As AjaxControlToolkit.ModalPopupExtender = WUC_consulta.FindControl("mpe_es_nhc")
            mpe_es_nhc.Show()
            hid_es_nhc.Value = "N"
        End If

    End Sub

    Protected Sub frm_telemed_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles frm_telemed.Init
        Dim datos_usuario As New WStelemed.cls_usuario
        datos_usuario = Session("datos_usuario")

        If datos_usuario Is Nothing Then
            Login1.Visible = True
            mpe_login.Enabled = True
            mpe_login.Show()
        Else
            mpe_login.Enabled = False
            Login1.Visible = False
        End If
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        If Not IsNothing(Session("datos_usuario")) Then
            datos_usuario = Session("datos_usuario")
        Else ' Sin dato inicializa al usuario
            datos_usuario = New WStelemed.cls_usuario
        End If

        If Not IsNothing(Session("datos_paciente")) Then
            datos_paciente = Session("datos_paciente")
        Else
            datos_paciente = New WStelemed.cls_paciente
        End If

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = Session("datos_informacion")
        Else
            datos_informacion = New WStelemed.cls_informacion
        End If

    End Sub

    Protected Sub Page_InitComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.InitComplete
        If Not IsNothing(Session("datos_usuario")) Then
            datos_usuario = Session("datos_usuario")
        Else ' Sin dato inicializa al usuario
            datos_usuario = New WStelemed.cls_usuario
        End If

        If Not IsNothing(Session("datos_paciente")) Then
            datos_paciente = Session("datos_paciente")
        Else
            datos_paciente = New WStelemed.cls_paciente
        End If

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = Session("datos_informacion")
        Else
            datos_informacion = New WStelemed.cls_informacion
        End If
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        If Not IsNothing(Session("datos_usuario")) Then
            datos_usuario = Session("datos_usuario")
        Else ' Sin dato inicializa al usuario
            datos_usuario = New WStelemed.cls_usuario
        End If

        If Not IsNothing(Session("datos_paciente")) Then
            datos_paciente = Session("datos_paciente")
        Else
            datos_paciente = New WStelemed.cls_paciente
        End If

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = Session("datos_informacion")
        Else
            datos_informacion = New WStelemed.cls_informacion
        End If
    End Sub

    Protected Sub Page_PreLoad(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreLoad
        If Not IsNothing(Session("datos_usuario")) Then
            datos_usuario = CType(Session("datos_usuario"), WStelemed.cls_usuario)
        End If
        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = CType(Session("datos_informacion"), WStelemed.cls_informacion)
        End If
        If Not IsNothing(Session("datos_paciente")) Then
            datos_paciente = CType(Session("datos_paciente"), WStelemed.cls_paciente)
        End If
    End Sub
End Class

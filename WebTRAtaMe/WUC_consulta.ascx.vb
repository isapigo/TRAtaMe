' ------------------------------------------------------------------------------
' Autora: Isabel Piñuel González 
' Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
' Universidad Internacional de la Rioja (UNIR)
' Este código se ofrece bajo licencia MIT
' ------------------------------------------------------------------------------
Imports System.Data
Imports System.Net

Partial Class WUC_consulta
    Inherits System.Web.UI.UserControl
    Private datos_usuario As New WStelemed.cls_usuario
    Private datos_paciente As New WStelemed.cls_paciente
    Private datos_informacion As New WStelemed.cls_informacion

    Protected Sub txt_episodio_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_episodio.TextChanged

        Dim WS As New WStelemed.telediagnosticoWS

        Dim mensaje As String = ""
        Dim color As Drawing.Color

        'Recupera los datos de un paciente

        Dim nomdoctor As String = ""
        Dim es_de As String = ""

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = CType(Session("datos_informacion"), WStelemed.cls_informacion)
        End If

        datos_informacion.codser = cmb_gfh.Text

        If WS.toda_informacion(txt_episodio.Text, datos_paciente, datos_informacion, mensaje) Then
            If rdb_video.Checked Then
                img_menos1.ToolTip = "Vídeo anterior"
                img_mas1.ToolTip = "Vídeo posterior"
                datos_informacion.veo_tipo = "V"
            End If
            If rdb_foto.Checked Then
                img_menos1.ToolTip = "Foto anterior"
                img_mas1.ToolTip = "Foto posterior"
                datos_informacion.veo_tipo = "F"
            End If
        End If

        Session("datos_informacion") = datos_informacion
        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje)
        Session("datos_paciente") = datos_paciente

        If Len(Trim(mensaje)) = 0 Then 'Si no viene con error
            If Len(Trim(datos_paciente.nombre_com)) > 0 Then
                mensaje = "Cumplimente la información."
                color = Drawing.Color.Green
            Else
                mensaje = "No existe información del paciente"
                color = Drawing.Color.Red
                btn_borra.Enabled = False
            End If
        End If

        btn_clic_rdb_Click(sender, e)
        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = CType(Session("datos_informacion"), WStelemed.cls_informacion)
        End If

        'Si el mensaje es muy largo lo troceamos
        Dim texto1 As String = ""
        Dim texto2 As String = ""
        Dim texto3 As String = ""
        funciones.trocear_texto(mensaje, texto1, texto2, texto3)
        funciones.mensaje(lbl_aviso_1, texto1, color, True, 12)
        funciones.mensaje(lbl_aviso_2, texto2, color, True, 12)
        funciones.mensaje(lbl_aviso_3, texto3, color, True, 12)
        hid_mostrar_aviso.Value = "S"

    End Sub

    Protected Sub btn_borra_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_borra.Click

        Dim WS As New WStelemed.telediagnosticoWS

        Dim mensaje As String = ""
        Dim tipo As String = "F"

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = CType(Session("datos_informacion"), WStelemed.cls_informacion)
        End If

        Try
            Using Conn As New SqlClient.SqlConnection
                Dim id_fichero As Integer = 0
                Select Case tipo
                    Case "A"
                        id_fichero = datos_informacion.oigo
                    Case Else
                        id_fichero = datos_informacion.veo
                End Select

                Dim str_SQL As String = "DELETE FROM fichero" & _
                                        " WHERE id = " & id_fichero & ";"


                Conn.ConnectionString = funciones.conexion_config("cadena_conexion_telemed").ConnectionString

                If Conn.State = ConnectionState.Closed Or _
                   Conn.State = ConnectionState.Broken Then
                    Conn.Open()
                End If

                Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)
                Dim filas As Integer = Sentencia.ExecuteNonQuery

                If filas = 1 Then

                    Dim pos As Integer = 0
                    Dim fic_ent As String = ""
                    ' Introducir la ip del ftp para intercambio de ficheros
                    Dim host As String = "ftp:ip_servidor_ftp"
                    Select Case tipo
                        Case "A"
                            pos = InStr(1, Trim(datos_informacion.oigo_enlace), "audio\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.oigo_enlace), pos)
                        Case "F"
                            pos = InStr(1, Trim(datos_informacion.veo_enlace), "foto\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                        Case "V"
                            pos = InStr(1, Trim(datos_informacion.veo_enlace), "video\", CompareMethod.Text)
                            fic_ent = Mid(Trim(datos_informacion.veo_enlace), pos)
                    End Select

                    fic_ent = Replace(fic_ent, "\", "/")

                    Dim request As FtpWebRequest = DirectCast(WebRequest.Create(New Uri(host & fic_ent)), FtpWebRequest)
                    request.Method = WebRequestMethods.Ftp.DeleteFile
                    request.Credentials = New NetworkCredential(UserName, Password)
                    request.UsePassive = False

                    WS.inserta_log("WebService", datos_informacion.id_episodio, "Antes de eliminar, fichero:" & fic_ent)

                    Dim response As FtpWebResponse
                    response = CType(request.GetResponse(), FtpWebResponse)
                    response.Close()

                    mensaje = ""
                Else
                    mensaje = "No se ha eliminado el fichero."
                End If

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            mensaje = ex.Message
            WS.inserta_log("WebService", datos_informacion.id_episodio, ex.Message)
        End Try

    End Sub

    Private Sub btn_limpiar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_limpiar.Click

        If datos_usuario.codser = "INF" Or datos_usuario.codser = "GRTH" Then
            cmb_gfh.Enabled = True
            cmb_gfh.Text = "TOD"
            btn_borra.Enabled = True
        Else
            cmb_gfh.Enabled = False
            cmb_gfh.Text = datos_usuario.codser
            btn_borra.Enabled = False
        End If

        cmb_gfh_pon_textos.ClearSelection()
        txt_fecha.Text = "01/01/1900"

        datos_paciente.nombre_com = ""
        datos_paciente.fecnac = Now()
        datos_informacion.id_episodio = 0
        datos_informacion.fecha = Now()
        datos_informacion.pregunta = ""
        datos_informacion.respuesta = ""
        datos_informacion.audios = 0
        datos_informacion.fotos = 0
        datos_informacion.videos = 0
        datos_informacion.veo = 0
        datos_informacion.veo_enlace = ""
        datos_informacion.oigo_enlace = ""
        datos_informacion.oigo = 0
        datos_informacion.fecha = "01/01/1900"

        datos_paciente.nhc = ""
        datos_paciente.dni = ""
        datos_paciente.nombre_com = ""
        datos_paciente.fecnac = Now()

        Session("datos_informacion") = datos_informacion
        Session("datos_paciente") = datos_paciente

        rellena_pantalla(datos_informacion, datos_paciente)

    End Sub

    Protected Sub btn_ver_pacientes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_ver_pacientes.Click

        hid_episodio.Value = cmb_pacientes.SelectedValue
        txt_episodio.Text = hid_episodio.Value
        txt_fecha.Text = Mid(cmb_pacientes.Items.Item(cmb_pacientes.SelectedIndex).Text, 9, 10)

        Dim pos = InStr(cmb_pacientes.Items.Item(hid_indserv.Value).Text, " - (", CompareMethod.Text) + 4
        Dim codser4 As String = Mid(cmb_pacientes.Items.Item(hid_indserv.Value).Text, pos, 4)
        Dim tam As Integer = 3

        If Right(codser4, 1) <> ")" Then tam = 4
        If codser4 <> ") - " Then cmb_gfh.Text = Mid(cmb_pacientes.Items.Item(hid_indserv.Value).Text, pos, tam)

        funciones.marca_servicios(cmb_gfh_pon_textos, hid_episodio.Value)

        txt_episodio_TextChanged(sender, e)

    End Sub

    Protected Sub btn_nhc_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_nhc.Click
        Dim mensaje As String = ""
        Dim color As Drawing.Color = Drawing.Color.Green

        If Not IsNothing(Session("datos_paciente")) Then
            datos_paciente = Session("datos_paciente")
        Else
            datos_paciente = New WStelemed.cls_paciente
        End If

        'Se valida el NHC
        If validar(mensaje, color, False) Then
            hid_es_nhc.Value = "S"
            'Si la validacion del nhc es correcta viene con la informacion a mostrar
            txt_es_nhc.Text = mensaje
        Else
            'Si el mensaje es muy largo lo troceamos
            Dim texto1 As String = ""
            Dim texto2 As String = ""
            Dim texto3 As String = ""
            funciones.trocear_texto(mensaje, texto1, texto2, texto3)
            funciones.mensaje(lbl_aviso_1, texto1, color, True, 12)
            funciones.mensaje(lbl_aviso_2, texto2, color, True, 12)
            funciones.mensaje(lbl_aviso_3, texto3, color, True, 12)
            hid_mostrar_aviso.Value = "S"
        End If

    End Sub

    Protected Sub btn_pacientes_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles btn_pacientes.Click
        Dim WS As New WStelemed.telediagnosticoWS

        Dim mensaje As String = ""
        Dim color As Drawing.Color
        hid_indserv.Value = 0

        Dim fecha_desde As Date
        Dim fecha_hasta As Date
        Select Case True
            Case (Not IsDate(txt_fecha.Text)) Or (txt_fecha.Text = "01/01/1900")
                fecha_desde = DateAdd(DateInterval.Day, -30, Now())
                fecha_hasta = Now()
            Case IsDate(txt_fecha.Text)
                fecha_desde = Format(CDate(txt_fecha.Text), "dd/MM/yyyy")
                fecha_hasta = Now()
        End Select

        Dim nhc As String = "AND 1=1  "
        If IsNumeric(txt_nhc.Text) Then
            nhc = "AND nhc = " & txt_nhc.Text & " "
        End If

        Using Conn As New SqlClient.SqlConnection
            ' Introducir la cadena de conexión a la base de datos de TRAtaMe
            Conn.ConnectionString = funciones.conexion_config("cadena_conexion_bd").ConnectionString

            Dim servicio As String = "AND 1=1 "
            If cmb_gfh.Text <> "TOD" Then servicio = "AND c.codser = '" & cmb_gfh.Text & "' "

            Dim str_SQL As String = "SELECT b.id as episodio, RTRIM(a.nombre) + ' '+ RTRIM(a.apellido1) + ' '+ RTRIM(a.apellido2) As nombre, b.fecha, c.codser, c.fecha, a.nhc " &
                                      "FROM paciente a, episodio b, respuesta c " &
                                     "WHERE b.fecha >= '" & Format(fecha_desde, "dd/MM/yyyy 00:00:00") & "' " &
                                       "AND b.fecha <= '" & Format(fecha_hasta, "dd/MM/yyyy 23:59:59") & "' " &
                                       servicio &
                                       nhc &
                                       "AND c.idepisodio = b.id " &
                                       "AND b.idpaciente = a.id "
            If servicio <> "AND 1=1 " Then
                str_SQL = str_SQL & "ORDER BY b.id desc, b.fecha desc, c.codser, a.nombre;"
            Else
                str_SQL = str_SQL & " UNION SELECT b.id as episodio, RTRIM(a.nombre) + ' '+ RTRIM(a.apellido1) + ' '+ RTRIM(a.apellido2) As nombre, b.fecha, '' as codser, NULL as fecha, a.nhc " &
                                          "FROM paciente a, episodio b " &
                                         "WHERE b.fecha >= '" & Format(fecha_desde, "dd/MM/yyyy 00:00:00") & "' " &
                                           "AND b.fecha <= '" & Format(fecha_hasta, "dd/MM/yyyy 23:59:59") & "' " &
                                           servicio &
                                           nhc &
                                           "AND b.id NOT IN (SELECT c.idepisodio FROM respuesta c) " &
                                           "AND b.idpaciente = a.id " &
                                         "ORDER BY 1 desc, 3 desc, 4, 2;"
            End If

            Using Devuelve As SqlClient.SqlDataReader = funciones.coge_datos_SQLSERVER(str_SQL, Conn)
                Try
                    Dim listax As String = ""
                    Dim listan As String = ""
                    Dim lin_id As Integer = 0
                    Dim codser As String = ""
                    If Devuelve.HasRows Then
                        hid_mostrar.Value = "S"
                        hid_mostrar_aviso.Value = "N"
                        Do While Devuelve.Read
                            Dim fecha As Date = Devuelve.GetValue(2)
                            Dim datoa As String = ""
                            Dim datob As String = ""
                            Dim infor As String = "RE:NO"

                            If IsDate(Devuelve.GetValue(4)) Then
                                infor = "RE:SI"
                            End If
                            If lin_id <> Devuelve.GetValue(0) Or codser <> Trim(Devuelve.GetValue(3)) Then
                                lin_id = Devuelve.GetValue(0)
                                codser = Trim(Devuelve.GetValue(3))
                                listax = listax & "[" & infor & " - " & Trim(Devuelve.GetValue(2)) & " - (" & Trim(Devuelve.GetValue(3)) & ")" & " - " & Trim(Devuelve.GetValue(1)) & "]"
                                listan = listan & "[" & Trim(Devuelve.GetValue(0)) & "]"
                            End If
                        Loop
                        funciones.carga_resto_combos(cmb_pacientes, listax, listan, "N")
                    Else
                        mensaje = "No existen pacientes con los criterios de selección elegidos."
                        color = Drawing.Color.Green

                        'Si el mensaje es muy largo lo troceamos
                        Dim texto1 As String = ""
                        Dim texto2 As String = ""
                        Dim texto3 As String = ""

                        funciones.trocear_texto(mensaje, texto1, texto2, texto3)
                        funciones.mensaje(lbl_aviso_1, texto1, color, True, 12)
                        funciones.mensaje(lbl_aviso_2, texto2, color, True, 12)
                        funciones.mensaje(lbl_aviso_3, texto3, color, True, 12)
                        hid_mostrar_aviso.Value = "S"
                        hid_mostrar.Value = "N"
                    End If

                    Devuelve.Close()

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    WS.inserta_log("ERROR", 0, "MENSAJE en Click de btn_pacientes:" & ex.Message)
                End Try

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using
        End Using

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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If hid_entra.Value = "N" Then
            btn_limpiar_Click(sender, e)
            If cmb_gfh.Items.Count = 0 Then funciones.carga_combo_gfh(cmb_gfh, "S")
            If cmb_gfh_pon_textos.Items.Count = 0 Then funciones.carga_combo_gfh(cmb_gfh_pon_textos, "N")
            hid_entra.Value = "S"
            rdb_foto.Checked = True
            rdb_video.Checked = False
        End If

        If Not Session("datos_usuario") Is Nothing Then
            datos_usuario = CType(Session("datos_usuario"), WStelemed.cls_usuario)
            If Len(txt_doctor.Text) = 0 Then
                txt_doctor.Text = datos_usuario.nombre
                hid_codemp.Value = datos_usuario.codemp
                If datos_usuario.codser = "INF" Or datos_usuario.codser = "GRTH" Then
                    cmb_gfh.Enabled = True
                    btn_borra.Enabled = True
                Else
                    cmb_gfh.Enabled = False
                    cmb_gfh.Text = datos_usuario.codser
                    btn_borra.Enabled = False
                End If
            End If
        End If

    End Sub

    Function rellena_pantalla(ByVal datos_informacion As WStelemed.cls_informacion, ByVal datos_paciente As WStelemed.cls_paciente) As Boolean

        If Not IsNothing(Session("datos_usuario")) Then
            datos_usuario = CType(Session("datos_usuario"), WStelemed.cls_usuario)
        End If

        Dim tam As Integer = Len(datos_paciente.nhc)

        If IsNumeric(datos_paciente.nhc) And tam > 0 And tam < 10 Then
            Dim WS As New WStelemed.telediagnosticoWS
            Dim mensaje As String = ""
            datos_paciente = WS.datos_paciente_HCIS(datos_paciente.nhc, datos_paciente.dni, mensaje)
            datos_paciente.id_paciente = WS.existe_paciente(datos_paciente.nhc, mensaje)
            If WS.actualiza_paciente(datos_paciente, mensaje) Then
                mensaje = "ACTUALIZACION CORRECTA"
            End If
        End If

        If tam = 10 Then
            btn_nhc.Visible = True
        Else
            btn_nhc.Visible = False
            txt_nhc.Text = datos_paciente.nhc
        End If

        txt_nombre.Text = datos_paciente.nombre_com
        txt_edad.Text = DateDiff(DateInterval.Year, datos_paciente.fecnac, Now)
        txt_episodio.Text = datos_informacion.id_episodio
        txt_fecha.Text = datos_informacion.fecha
        txt_pregunta.Text = datos_informacion.pregunta
        txt_respuesta.Text = datos_informacion.respuesta
        lbl_audio.Text = datos_informacion.audios & " Audios"
        lbl_foto.Text = datos_informacion.fotos & " Fotos"
        lbl_video.Text = datos_informacion.videos & " Vídeos"
        lbl_id_media.Text = datos_informacion.veo

        txt_pon_textos.Text = ""
        Select Case datos_informacion.veo_tipo
            Case "F"
                ifm_multimed.Attributes("src") = datos_informacion.veo_enlace
                ifm_multimed.Attributes("width") = "660"
                ifm_multimed.Attributes("height") = "350"
                vid_multimed.Attributes("src") = ""
                vid_multimed.Attributes("width") = "0"
                vid_multimed.Attributes("height") = "0"
                vid_multimed.Attributes("controls") = "False"
                vid_multimed.Attributes("autoplay") = "False"
                vid_multimed.Attributes("loop") = "False"
            Case "V"
                vid_multimed.Attributes("src") = datos_informacion.veo_enlace
                vid_multimed.Attributes("type") = "'video/mp4"
                vid_multimed.Attributes("codecs") = "avc1,mp4a"
                vid_multimed.Attributes("width") = "660"
                vid_multimed.Attributes("height") = "350"
                vid_multimed.Attributes("controls") = "True"
                vid_multimed.Attributes("autoplay") = "True"
                vid_multimed.Attributes("loop") = "True"
                ifm_multimed.Attributes("src") = ""
                ifm_multimed.Attributes("width") = "0"
                ifm_multimed.Attributes("height") = "0"
        End Select

        If Not IsNothing(datos_usuario) And Not IsNothing(datos_informacion.pregunta) Then
            If datos_informacion.audios > 0 Then
                btn_play.Enabled = True
                aud_play.Attributes("src") = datos_informacion.oigo_enlace
                If datos_informacion.audios > 1 Then
                    img_amas1.Enabled = True
                    img_amenos1.Enabled = True
                Else
                    img_amas1.Enabled = False
                    img_amenos1.Enabled = False
                End If
            Else
                aud_play.Attributes("src") = ""
                btn_play.Enabled = False
                img_amas1.Enabled = False
                img_amenos1.Enabled = False
            End If
            If (datos_usuario.codser = "INF" Or datos_usuario.codser = "GRTH") Then
                btn_respuesta.Enabled = True
                btn_pregunta.Enabled = True
            Else
                btn_respuesta.Enabled = True
                btn_pregunta.Enabled = False
            End If
        Else
            btn_respuesta.Enabled = False
            btn_pregunta.Enabled = False
        End If

    End Function

    Function montar_ayuda(ByVal objeto As String) As Boolean

        Dim lbl As Label = Me.FindControl(Replace(objeto, "btn", "lbl"))

        Dim valor As String = funciones.leer_parametro(objeto)
        Dim a = Math.Round(Len(valor) / 33)

        mostrar_ayuda(Len(lbl.Text) * 10, _
              a * 50 + 80, _
              vbCrLf & vbCrLf & valor, _
              lbl.Text)
    End Function

    Function mostrar_ayuda(ByVal ancho As Integer, _
                           ByVal alto As Integer, _
                           ByVal texto As String, _
                           ByVal cabecera As String) As Boolean

        If ancho < 300 Then ancho = 300
        pan_ayuda.Width = ancho
        txt_ayuda.Width = ancho - 20
        lbl_ayuda_cab.Width = ancho
        pan_ayuda.Height = alto
        txt_ayuda.Height = alto - 130
        txt_ayuda.Text = texto
        funciones.mensaje(lbl_ayuda_cab, cabecera, Drawing.Color.RoyalBlue, True, 12)
        hid_mostrar_ayuda.Value = "S"

        'mpe_ayuda.Show()

    End Function

    Function validar(ByRef mensaje As String, ByRef color As Drawing.Color, ByVal todo As Boolean) As Boolean

        'Se valida el NHC
        If Not IsNumeric(txt_nhc.Text) Then
            mensaje = "El número de historia es obligatorio y numérico."
            color = Drawing.Color.Red
            Return False
        End If

        Dim WS As New WStelemed.telediagnosticoWS

        Dim datos_paciente_HCIS As New WStelemed.cls_paciente

        datos_paciente_HCIS = WS.datos_paciente_HCIS(txt_nhc.Text, "SIN_DNI", mensaje)

        If Trim(datos_paciente_HCIS.nhc) = "0" Then
            mensaje = "El NHC: " & Trim(txt_nhc.Text) & " No existe en la BBDD del hospital."
            color = Drawing.Color.Red
            Return False
        Else
            lbl_es_nhc.Text = "Comprobar NHC: " & Trim(txt_nhc.Text)
            mensaje = "Compruebe que la información recogida en" & Constants.vbCrLf & _
                      "nuestros sistemas se corresponda al paciente" & Constants.vbCrLf & _
                      "a diagnosticar." & Constants.vbCrLf & Constants.vbCrLf & _
                      "Fecha Nacimiento: " & Trim(datos_paciente_HCIS.fecnac) & Constants.vbCrLf & _
                  Trim(datos_paciente_HCIS.nombre_com) & Constants.vbCrLf & _
                  datos_paciente_HCIS.domicilio & Constants.vbCrLf & _
                  Trim(datos_paciente_HCIS.codpost) & " - " & Trim(datos_paciente_HCIS.municipio)
        End If

        Return True

    End Function

    Protected Sub btn_clic_rdb_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_clic_rdb.Click

        Dim mensaje As String = ""

        Dim WS As New WStelemed.telediagnosticoWS

        If txt_episodio.Text = "" Then Exit Sub

        If WS.toda_informacion(txt_episodio.Text, datos_paciente, datos_informacion, mensaje) Then
            If rdb_video.Checked Then
                img_menos1.ToolTip = "Vídeo anterior"
                img_mas1.ToolTip = "Vídeo posterior"
                datos_informacion.veo_tipo = "V"
            End If
            If rdb_foto.Checked Then
                img_menos1.ToolTip = "Foto anterior"
                img_mas1.ToolTip = "Foto posterior"
                datos_informacion.veo_tipo = "F"
            End If
        End If

        Session("datos_informacion") = datos_informacion
        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje)
        Session("datos_paciente") = datos_paciente

        datos_informacion.veo_enlace = ""

        datos_informacion = WS.son_multimedia(datos_informacion)

        Session("datos_informacion") = datos_informacion

        Dim el_id_MIN As Integer = WS.dame_id_multimed(datos_informacion.veo_tipo, datos_informacion, "MIN")
        datos_informacion = WS.dame_img_aud(datos_informacion.veo_tipo, " id = " & el_id_MIN & " Order by id", datos_informacion)

        Session("datos_paciente") = WS.datos_paciente(datos_informacion.id_episodio, 0, mensaje)

        rellena_pantalla(datos_informacion, Session("datos_paciente"))

    End Sub

    Protected Sub img_menos1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles img_menos1.Click

        Dim WS As New WStelemed.telediagnosticoWS

        Dim el_where As String = ""
        Dim mensaje As String = ""

        If WS.toda_informacion(txt_episodio.Text, datos_paciente, datos_informacion, mensaje) Then
            If rdb_video.Checked Then
                img_menos1.ToolTip = "Vídeo anterior"
                img_mas1.ToolTip = "Vídeo posterior"
                datos_informacion.veo_tipo = "V"
            End If
            If rdb_foto.Checked Then
                img_menos1.ToolTip = "Foto anterior"
                img_mas1.ToolTip = "Foto posterior"
                datos_informacion.veo_tipo = "F"
            End If
        End If

        Session("datos_informacion") = datos_informacion
        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje)
        Session("datos_paciente") = datos_paciente

        Dim control As String = DirectCast(sender, System.Web.UI.WebControls.ImageButton).ClientID
        Dim tipo As String = "F"
        Dim veo_oigo As Integer = 0

        Select Case control
            Case "WUC_consulta_img_amenos1"
                tipo = "A"
                veo_oigo = datos_informacion.oigo
            Case "WUC_consulta_img_menos1"
                tipo = datos_informacion.veo_tipo
                veo_oigo = lbl_id_media.Text
        End Select

        Dim el_id_MAX As Integer = WS.dame_id_multimed(tipo, datos_informacion, "MAX")
        Dim el_id_MIN As Integer = WS.dame_id_multimed(tipo, datos_informacion, "MIN")

        Select Case True
            Case el_id_MIN < veo_oigo
                el_where = "id < " & veo_oigo
            Case el_id_MAX >= veo_oigo
                el_where = "id = " & el_id_MAX
        End Select

        el_where = el_where & " Order by id desc"

        Try
            datos_informacion = WS.dame_img_aud(tipo, el_where, datos_informacion)
            datos_informacion = WS.son_multimedia(datos_informacion)

            Session("datos_informacion") = datos_informacion

            Session("datos_paciente") = WS.datos_paciente(datos_informacion.id_episodio, 0, mensaje)

            rellena_pantalla(datos_informacion, Session("datos_paciente"))

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Protected Sub img_mas1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles img_mas1.Click

        Dim WS As New WStelemed.telediagnosticoWS

        Dim el_where As String = ""
        Dim mensaje As String = ""

        If WS.toda_informacion(txt_episodio.Text, datos_paciente, datos_informacion, mensaje) Then
            If rdb_video.Checked Then
                img_menos1.ToolTip = "Vídeo anterior"
                img_mas1.ToolTip = "Vídeo posterior"
                datos_informacion.veo_tipo = "V"
            End If
            If rdb_foto.Checked Then
                img_menos1.ToolTip = "Foto anterior"
                img_mas1.ToolTip = "Foto posterior"
                datos_informacion.veo_tipo = "F"
            End If
        End If

        Session("datos_informacion") = datos_informacion
        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje)
        Session("datos_paciente") = datos_paciente

        Dim control As String = DirectCast(sender, System.Web.UI.WebControls.ImageButton).ClientID
        Dim tipo As String = "F"
        Dim veo_oigo As Integer = 0

        Select Case control
            Case "WUC_consulta_img_amas1"
                tipo = "A"
                veo_oigo = datos_informacion.oigo
            Case "WUC_consulta_img_mas1"
                tipo = datos_informacion.veo_tipo
                veo_oigo = lbl_id_media.Text
        End Select

        Try

            Dim el_id_MAX As Integer = WS.dame_id_multimed(tipo, datos_informacion, "MAX")
            Dim el_id_MIN As Integer = WS.dame_id_multimed(tipo, datos_informacion, "MIN")

            Select Case True
                Case el_id_MAX > veo_oigo
                    el_where = "id > " & veo_oigo
                Case el_id_MIN <= veo_oigo
                    el_where = "id = " & el_id_MIN
            End Select

            el_where = el_where & " Order by id;"

            datos_informacion = WS.son_multimedia(datos_informacion)
            datos_informacion = WS.dame_img_aud(tipo, el_where, datos_informacion)

            Session("datos_informacion") = datos_informacion

            Session("datos_paciente") = WS.datos_paciente(datos_informacion.id_episodio, 0, mensaje)

            rellena_pantalla(datos_informacion, Session("datos_paciente"))

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Protected Sub btn_pregunta_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_pregunta.Click
        lbl_texto_libre.Text = lbl_pregunta.Text
        funciones.marca_servicios(cmb_gfh_pon_textos, txt_episodio.Text)
        cmb_gfh_pon_textos.Enabled = True
        hid_mostrar_textos.Value = "S"
    End Sub

    Protected Sub btn_respuesta_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_respuesta.Click
        lbl_texto_libre.Text = lbl_respuesta.Text
        funciones.marca_servicios(cmb_gfh_pon_textos, txt_episodio.Text)
        cmb_gfh_pon_textos.Enabled = False
        hid_mostrar_textos.Value = "S"
    End Sub

    Protected Sub btn_pon_textos_aceptar_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_pon_textos_aceptar.Click

        Dim WS As New WStelemed.telediagnosticoWS
        Dim color = Drawing.Color.Green
        Dim mensaje As String = ""
        Dim str_SQL As String = ""

        Dim son As Integer = cmb_gfh_pon_textos.Items.Count - 1
        Dim lista_no(son) As String
        Dim lista_si(son) As String
        Dim lista_ya(son) As String

        If WS.toda_informacion(txt_episodio.Text, datos_paciente, datos_informacion, mensaje) Then
            If rdb_video.Checked Then
                img_menos1.ToolTip = "Vídeo anterior"
                img_mas1.ToolTip = "Vídeo posterior"
                datos_informacion.veo_tipo = "V"
            End If
            If rdb_foto.Checked Then
                img_menos1.ToolTip = "Foto anterior"
                img_mas1.ToolTip = "Foto posterior"
                datos_informacion.veo_tipo = "F"
            End If
        End If

        Session("datos_informacion") = datos_informacion
        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje)
        Session("datos_paciente") = datos_paciente


        For i = 0 To son
            lista_si(i) = ""
            lista_no(i) = ""
            lista_ya(i) = ""
            Dim gfh As String = WS.dame_GFH(Trim(cmb_gfh_pon_textos.Items.Item(i).Text))
            If cmb_gfh_pon_textos.Items.Item(i).Selected Then lista_si(i) = gfh
        Next

        If Len(txt_pon_textos.Text) = 0 Then
            mensaje = "No ha realizado aportaciones."
            color = Drawing.Color.Red
        Else
            Dim por As String = datos_usuario.nombre
            Select Case True
                Case lbl_texto_libre.Text = lbl_respuesta.Text
                    datos_informacion.fch_resp = Format(Now(), "dd/MM/yyyy HH:mm:ss")
                    datos_informacion.respuesta = "El " & datos_informacion.fch_resp & Constants.vbCrLf & Trim(txt_pon_textos.Text) & _
                                                  Constants.vbCrLf & por & Constants.vbCrLf & datos_informacion.respuesta & Constants.vbCrLf
                    str_SQL = "UPDATE respuesta SET " & _
                               "usuario = '" & datos_usuario.codemp & "', doctor = '" & datos_usuario.nombre & "', " & _
                               "fecha = '" & datos_informacion.fch_resp & "', respuesta = '" & datos_informacion.respuesta & "' " & _
                               "WHERE idepisodio = '" & datos_informacion.id_episodio & "' AND codser = '" & datos_informacion.codser & "';"

                Case lbl_texto_libre.Text = lbl_pregunta.Text
                    For i = 0 To son
                        lista_si(i) = ""
                        lista_no(i) = ""
                        lista_ya(i) = ""
                        Dim gfh As String = WS.dame_GFH(Trim(cmb_gfh_pon_textos.Items.Item(i).Text))
                        If cmb_gfh_pon_textos.Items.Item(i).Selected Then lista_si(i) = gfh
                    Next

                    WS.control_respuestas(datos_informacion.id_episodio, lista_si, lista_no, lista_ya, mensaje)

                    For i = 0 To son
                        If lista_no(i) <> "" Then WS.elimina_pregunta(datos_informacion.id_episodio, WS.dame_GFH(lista_no(i)), mensaje)
                        If lista_si(i) = "" Then Continue For
                        Dim manda As Boolean = True
                        For x = 0 To son
                            If lista_si(i) = WS.dame_GFH(lista_ya(x)) Then manda = False
                        Next
                        If manda Then WS.preguntar_a_gfh(datos_informacion.id_episodio, lista_si(i), mensaje)
                    Next

                    Dim fecha As String = Format(Now(), "dd/MM/yyyy HH:mm:ss")
                    datos_informacion.pregunta = "El " & fecha & Constants.vbCrLf & Trim(txt_pon_textos.Text) & _
                                                  Constants.vbCrLf & por & Constants.vbCrLf & datos_informacion.pregunta & Constants.vbCrLf
                    str_SQL = "UPDATE episodio SET " & _
                               "pregunta = '" & datos_informacion.pregunta & "' " & _
                               "WHERE id = '" & datos_informacion.id_episodio & "';" & _
                              "UPDATE respuesta SET " & _
                               "fecha = NULL " & _
                               "WHERE idepisodio = '" & datos_informacion.id_episodio & "';"

            End Select
            If WS.grabar_textos(datos_usuario, datos_paciente, datos_informacion, str_SQL, mensaje) = False Then
                color = Drawing.Color.Red
            Else
                txt_respuesta.Text = datos_informacion.respuesta
                txt_pregunta.Text = datos_informacion.pregunta
                color = Drawing.Color.Green
            End If
        End If

        'Si el mensaje es muy largo lo troceamos
        Dim texto1 As String = ""
        Dim texto2 As String = ""
        Dim texto3 As String = ""

        funciones.trocear_texto(mensaje, texto1, texto2, texto3)
        funciones.mensaje(lbl_aviso_1, texto1, color, True, 12)
        funciones.mensaje(lbl_aviso_2, texto2, color, True, 12)
        funciones.mensaje(lbl_aviso_3, texto3, color, True, 12)
        hid_mostrar_aviso.Value = "S"

        If color = Drawing.Color.Red Then
            WS.inserta_log("ERROR", 0, "MENSAJE en btn_pon_textos_aceptar_Click:" & mensaje)
        End If

    End Sub

    Protected Sub btn_play_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles btn_play.Click

        Dim WS As New WStelemed.telediagnosticoWS

        Dim str_SQL As String = ""

        If Not IsNothing(Session("datos_informacion")) Then
            datos_informacion = CType(Session("datos_informacion"), WStelemed.cls_informacion)
        End If

        If txt_episodio.Text = "" Then Exit Sub

        datos_informacion.oigo_enlace = ""

        str_SQL = "SELECT id, enlace" & _
                    " FROM fichero" & _
                    " WHERE idepisodio = " & datos_informacion.id_episodio & _
                    " AND tipofic = 'A'" & _
                    " Order by id"
        Try
            Using Conn As New SqlClient.SqlConnection
                datos_informacion.oigo_enlace = ""
                ' Indicad la cadena de conexión a la base de datos TRAtaMe
                Conn.ConnectionString = funciones.conexion_config("cadena_conexion_TRAtaMe").ConnectionString
                Dim Devuelve As SqlClient.SqlDataReader = funciones.coge_datos_SQLSERVER(str_SQL, Conn)

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        datos_informacion.oigo = Devuelve.GetValue(0)
                        datos_informacion.oigo_enlace = Devuelve.GetValue(1)
                        Exit Do
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

                Session("datos_informacion") = datos_informacion

                Dim mensaje = ""
                Session("datos_paciente") = WS.datos_paciente(datos_informacion.id_episodio, 0, mensaje)

                rellena_pantalla(datos_informacion, Session("datos_paciente"))

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Protected Sub img_amas1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles img_amas1.Click
        img_mas1_Click(sender, e)
    End Sub

    Protected Sub img_amenos1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles img_amenos1.Click
        img_menos1_Click(sender, e)
    End Sub

    Protected Sub btn_es_nhc_si_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_es_nhc_si.Click

        Dim WS As New WStelemed.telediagnosticoWS
        Dim mensaje As String = ""
        Dim id_paciente As Integer

        datos_paciente = WS.datos_paciente(txt_episodio.Text, 0, mensaje) 'Aqui recupera informcion del paciente por el episodio

        id_paciente = datos_paciente.id_paciente
        datos_paciente = WS.datos_paciente_HCIS(Trim(txt_nhc.Text), "SIN_DNI", mensaje) 'Ahora buscamos al paciente por el nhc bueno en HCIS
        datos_paciente.id_paciente = id_paciente 'va con el id_paciente bueno recuperado 2 pasos antes

        If WS.actualiza_paciente(datos_paciente, mensaje) Then
            mensaje = "ACTUALIZACION CORRECTA del paciente con id: " & id_paciente & " al NHC: " & Trim(datos_paciente.nhc)
            Session("datos_paciente") = datos_paciente
            btn_nhc.Visible = False
        Else
            mensaje = "Error asignando NHC al paciente con id: " & id_paciente & " al NHC: " & Trim(datos_paciente.nhc)
        End If

        WS.inserta_log("ERROR", 0, "MENSAJE btn_es_nhc_si_Click:" & mensaje)

    End Sub

End Class

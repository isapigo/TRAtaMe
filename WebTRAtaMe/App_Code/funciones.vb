
Imports Microsoft.VisualBasic
Imports System.Data
' ------------------------------------------------------------------------------
' Autora: Isabel Piñuel González 
' Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
' Universidad Internacional de la Rioja (UNIR)
' Este código se ofrece bajo licencia MIT
' ------------------------------------------------------------------------------
Imports System.IO
Imports System.Net

Public Class funciones

    Public Shared Function mensaje(ByVal etiqueta As Label, _
                                   Optional ByVal texto As String = "", _
                                   Optional ByVal color_texto As Object = 0, _
                                   Optional ByVal color_fondo As Object = "#996600", _
                                   Optional ByVal visible As Boolean = False, _
                                   Optional ByVal tamaño_texto As Integer = 12) As Boolean

        If TypeOf (color_texto) Is Drawing.Color Then
            etiqueta.ForeColor = color_texto
        End If
        If TypeOf (color_fondo) Is Drawing.Color Then
            etiqueta.BackColor = color_fondo
        End If

        etiqueta.Text = texto
        etiqueta.Font.Size = tamaño_texto
        etiqueta.Visible = visible

    End Function

    Public Shared Function coge_empleado_m4(ByRef datos_usuario As WStelemed.cls_usuario) As Boolean
        Dim WS As New WStelemed.telediagnosticoWS

        Using Conn As New System.Data.Odbc.OdbcConnection
            Dim str_SQL As String

            Dim data_meta4 As New System.Web.UI.WebControls.SqlDataSource
            data_meta4 = funciones.conexion_config("cadena_conexion_meta4")

            coge_empleado_m4 = False
            Try
                str_SQL = "SELECT DISTINCT  informix.m4t_empleados.apellido_1 || ' ' || informix.m4t_empleados.apellido_2 || ', ' || informix.m4t_empleados.nombre, informix.m4t_empleados.id_legal, " & _
                                 "informix.m4t_puest_trab.n_puesto, informix.m4t_unid_org.n_unidad, informix.m4t_unid_org.id_unidad, informix.m4t_hist_puestos.fec_inicio, " & _
                                 "informix.m4t_empleados.fec_nacimiento, informix.m4t_puest_trab.id_categoria, informix.m4t_empleados.id_empleado " & _
                            "FROM informix.m4t_empleados, informix.m4t_hist_puestos, informix.m4t_puest_trab, informix.m4t_unid_org " & _
                           "WHERE (informix.m4t_empleados.id_empleado = '" & Trim(datos_usuario.codemp) & "' " & _
                              "OR informix.m4t_empleados.id_legal = '" & Trim(datos_usuario.dni) & "') " & _
                             "AND informix.m4t_empleados.id_empleado = informix.m4t_hist_puestos.id_empleado " & _
                             "AND informix.m4t_hist_puestos.id_puesto = informix.m4t_puest_trab.id_puesto " & _
                             "AND informix.m4t_hist_puestos.id_unidad = informix.m4t_unid_org.id_unidad " & _
                             "AND (informix.m4t_hist_puestos.fec_fin Is NULL " & _
                              "OR informix.m4t_hist_puestos.fec_fin >= '" & Format(Now(), "dd/MM/yyyy") & "') " & _
                           "ORDER BY informix.m4t_hist_puestos.fec_inicio DESC"

                Using Devuelve As Odbc.OdbcDataReader = funciones.coge_datos_ODBC(data_meta4, str_SQL, Conn)

                    If Devuelve.HasRows Then
                        Devuelve.Read()
                        datos_usuario.nombre = WS.mira_nulo(Devuelve.GetValue(0))
                        datos_usuario.dni = WS.mira_nulo(Devuelve.GetValue(1))
                        datos_usuario.puesto = WS.mira_nulo(Devuelve.GetValue(2))
                        datos_usuario.unidad = WS.mira_nulo(Devuelve.GetValue(3))
                        datos_usuario.codser = WS.mira_nulo(Devuelve.GetValue(4))
                        datos_usuario.codemp = WS.mira_nulo(Devuelve.GetValue(8))
                        datos_usuario.codigo = datos_usuario.codemp
                        coge_empleado_m4 = vale_codigo_permiso(Devuelve.GetValue(7))

                        If coge_empleado_m4 Then
                            datos_usuario.permiso = WS.mira_nulo(Devuelve.GetValue(7))
                        End If
                    Else
                        coge_empleado_m4 = False
                    End If

                    Devuelve.Close()

                End Using

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                WS.inserta_log("ERROR", 0, "MENSAJE en control_entrada:" & ex.Message)
            End Try

            If Conn.State = ConnectionState.Open Then
                Conn.Close()
            End If

        End Using

    End Function

    Public Shared Function coge_datos_ODBC(ByVal datos As SqlDataSource, _
                                           ByVal str_SQL As String, _
                                           ByVal Conn As System.Data.Odbc.OdbcConnection) As Odbc.OdbcDataReader
        Conn.ConnectionString = datos.ConnectionString
        If Conn.State = ConnectionState.Closed Then
            Conn.Open()
        End If

        Dim Sentencia As New Odbc.OdbcCommand(str_SQL, Conn)
        Dim Devuelve As Odbc.OdbcDataReader = Sentencia.ExecuteReader
        Return Devuelve

    End Function

    Public Shared Function conexion_config(ByVal texto As String) As System.Web.UI.WebControls.SqlDataSource

        conexion_config = New System.Web.UI.WebControls.SqlDataSource

        conexion_config.Dispose()
        conexion_config.ConnectionString = ConfigurationManager.ConnectionStrings(texto).ConnectionString
        conexion_config.ProviderName = ConfigurationManager.ConnectionStrings(texto).ProviderName
        conexion_config.CacheDuration = 1
        conexion_config.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding
        conexion_config.DataBind()
        conexion_config.DataSourceMode = SqlDataSourceMode.DataReader

        Return conexion_config

    End Function

    Public Shared Function carga_combo_gfh(ByRef cmb_gfh As Object, ByVal blanco_el_1 As String) As Boolean

        carga_combo_gfh = True

        'Para DropDownList y ListBox

        Using Conn As New SqlClient.SqlConnection
            Conn.ConnectionString = funciones.conexion_config("cadena_conexion_telemed").ConnectionString
            Dim str_SQL As String

            Try
                str_SQL = "SELECT  descripcion, codser " & _
                            "FROM servicio " & _
                            "ORDER BY descripcion"

                Using Devuelve As SqlClient.SqlDataReader = funciones.coge_datos_SQLSERVER(str_SQL, Conn)
                    'Si viene con S pone el primero a blanco
                    If blanco_el_1 = "S" Then
                        Dim dato As New ListItem("TODOS SERVICIOS", "TOD")
                        cmb_gfh.Items.Add(dato)
                    End If

                    If Devuelve.HasRows Then
                        Do While Devuelve.Read
                            Dim dato As New ListItem(Devuelve.GetValue(0), Devuelve.GetValue(1))
                            cmb_gfh.Items.Add(dato)
                        Loop
                    End If

                    Devuelve.Close()

                End Using

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                carga_combo_gfh = False
            End Try

            If Conn.State = ConnectionState.Open Then
                Conn.Close()
            End If

        End Using

        Return carga_combo_gfh

    End Function

    Public Shared Function carga_resto_combos(ByRef Control As Object, _
                                          ByVal lista_valores As String, _
                                          ByVal lista_indices As String, _
                                          ByVal blanco_el_1 As String) As Boolean

        '============================================
        'ATENCION ESTA ES LA CARGA DE COMBOS DIVERSOS
        '============================================
        Dim desde As Integer = 2
        Dim desden As Integer = 2
        Control.Items.Clear()

        'Si viene con S pone el primero a blanco
        If blanco_el_1 = "S" Then
            If Len(Trim(lista_indices)) = 0 Then
                Dim dato1 As New ListItem(" ")
                Control.Items.Add(dato1)
            Else
                Dim dato1 As New ListItem(" ", 0)
                Control.Items.Add(dato1)
            End If
        End If

        While InStr(desde, lista_valores, "]", CompareMethod.Text) > 0
            Dim hasta As Integer = InStr(desde, lista_valores, "]", CompareMethod.Text)
            Dim num_car As Integer = hasta - desde

            Dim valorx As String = Mid(lista_valores, desde, num_car)

            'Si viene con indices se añaden al combo
            If Len(Trim(lista_indices)) = 0 Then
                Dim dato As New ListItem(valorx)
                Control.Items.Add(dato)
            Else
                Dim hastan As Integer = InStr(desden, lista_indices, "]", CompareMethod.Text)
                Dim num_carn As Integer = hastan - desden
                Dim valorn As String = Mid(lista_indices, desden, num_carn)
                Dim dato As New ListItem(valorx, CInt(valorn))
                Control.Items.Add(dato)
                desden = hastan + 2
            End If

            desde = hasta + 2
        End While

        Return True

    End Function

    Public Shared Function marca_servicios(ByRef cmb_servicios As CheckBoxList, ByVal episodio As Integer) As Boolean
        Dim WS As New WStelemed.telediagnosticoWS
        Dim mensaje As String = ""
        Dim listax As String = ""
        Dim listan As String = ""
        Dim desde As Integer = 2

        cmb_servicios.ClearSelection()

        If WS.dame_servicios_episodio(episodio, mensaje, listax, listan) Then
            While desde < Len(Trim(listax))
                Dim hasta As Integer = InStr(desde, listax, "[", CompareMethod.Text)
                If hasta = 0 Then hasta = Len(Trim(listax)) + 1
                Dim num_car As Integer = hasta - desde

                Dim valorx As String = Mid(listax, desde, num_car)

                For a = 0 To cmb_servicios.Items.Count - 1
                    If Trim(cmb_servicios.Items.Item(a).Text) = Trim(valorx) Then
                        cmb_servicios.Items.Item(a).Selected = True
                    End If
                Next
                desde = hasta + 1
            End While

        End If

        Return True
    End Function

    Public Shared Function coge_datos_SQLSERVER(ByVal str_SQL As String, _
                                                ByVal Conn As SqlClient.SqlConnection) As SqlClient.SqlDataReader

        Dim Sentencia As New SqlClient.SqlCommand(str_SQL, Conn)

        If Conn.State = ConnectionState.Broken Or _
           Conn.State = ConnectionState.Closed Then
            Conn.Open()
        End If

        Dim Devuelve As SqlClient.SqlDataReader = Sentencia.ExecuteReader
        Return Devuelve

    End Function

    Public Shared Function exporta_log(ByVal Conn As SqlClient.SqlConnection) As Boolean
        Dim str_SQL As String

        Dim fichero As String
        Dim datos As String
        Dim anomes As String
        Dim anomes_ant As String
        Dim fecha As Date
        Dim fs, f As Object
        Dim WS As New WStelemed.telediagnosticoWS

        Try
            fecha = Today
            anomes_ant = Format(fecha, "yyyyMM")
            anomes = anomes_ant

            While anomes = anomes_ant
                fecha = DateAdd(DateInterval.Day, -1, fecha)
                anomes = Format(fecha, "yyyyMM")
            End While

            'Datos de conexion en web.config
            'Conn.Open() Ya viene abierto

            fichero = "\\" & Left(Conn.DataSource, InStrRev(Conn.DataSource, "\")) & Conn.Database & "\" & anomes & ".log"

            'Si existe el fichero no se trata
            If Len(Dir(fichero)) = 0 Then

                fecha = CStr(Format(fecha, "dd/MM/yyyy")) & " 23:59:59"

                FileOpen(255, fichero, OpenMode.Output, OpenAccess.Write, OpenShare.Shared)

                str_SQL = "SELECT * FROM log WHERE fecha_hora <= '" & Format(fecha, "dd/MM/yyyy") & "' ORDER BY id;"

                Dim Devuelve As SqlClient.SqlDataReader = coge_datos_SQLSERVER(str_SQL, Conn)

                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        datos = WS.mira_nulo(Devuelve.GetValue(0)) & "| " & WS.mira_nulo(Devuelve.GetValue(1)) & "| " & WS.mira_nulo(Devuelve.GetValue(2)) & "| " & WS.mira_nulo(Devuelve.GetValue(3)) & "| " & WS.mira_nulo(Devuelve.GetValue(4))
                        WriteLine(255, datos)
                    Loop
                End If

                FileClose(255)

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

                'Una vez tratados eliminamos los registros de la tabla log
                str_SQL = "DELETE FROM log WHERE fecha_hora <= '" & fecha & "';"

                Dim id As Integer
                Dim mensaje As String = ""
                tratamiento_SQL(Conn, str_SQL, "log", id, mensaje)

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

                fs = CreateObject("Scripting.FileSystemObject")
                f = fs.GetFile(fichero)
                f.Attributes = 1 'ReadOnly

            End If

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            WS.inserta_log("ERROR", 0, "MENSAJE en exporta_log:" & ex.Message)
        End Try

    End Function

    Public Shared Function tratamiento_SQL(ByVal Conn As SqlClient.SqlConnection, _
                                           ByVal str_SQL As String, _
                                           ByVal tabla As String, _
                                           ByRef id As Integer, _
                                           ByRef mensaje As String) As Boolean

        Dim WS As New WStelemed.telediagnosticoWS
        ' Inserta registro
        Dim Sentencia As New SqlClient.SqlCommand(str_SQL)

        If Conn.State = ConnectionState.Broken Or _
           Conn.State = ConnectionState.Closed Then
            Conn.Open()
        End If

        ' Habilita la nueva conexion OleDbConnection.
        Sentencia.Connection = Conn

        ' Abre la conexion e Inserta o Borra
        Try
            If Sentencia.ExecuteNonQuery() = 0 Then
                tratamiento_SQL = False
            Else
                'Recupera el autonumerico, para aquellas sql que lo precisan
                If Len(tabla) > 0 Then
                    Sentencia.CommandText = "SELECT @@IDENTITY FROM " & tabla & ";"
                    id = Sentencia.ExecuteScalar()
                End If
                tratamiento_SQL = True
            End If

            Sentencia.Connection.Close()

        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensaje = ex.Message
            WS.inserta_log("ERROR", 0, "MENSAJE en tratamiento_SQL:" & ex.Message & " " & str_SQL)
            tratamiento_SQL = False
        End Try

    End Function

    Public Shared Function valida_entrada(ByVal UserName As String, ByVal Password As String, ByRef lit_error As String, ByRef datos_usuario As WStelemed.cls_usuario) As Boolean
        Dim WS As New WStelemed.telediagnosticoWS
        Dim USUARIO As New WStelemed.telediagnosticoWS
        Dim el_usuario As New WStelemed.cls_usuario

        valida_entrada = False

        el_usuario = USUARIO.valida_entrada(UserName, Password)

        If el_usuario.permiso > 0 Then

            With el_usuario
                datos_usuario.codemp = .codemp
                datos_usuario.nombre = .nombre
                datos_usuario.codser = .codser
                datos_usuario.permiso = .permiso
                datos_usuario.origen = .origen
                datos_usuario.dni = .dni
                datos_usuario.codemp = .codemp
                datos_usuario.unidad = .unidad
                datos_usuario.puesto = .puesto
                datos_usuario.codigo = .codigo
            End With

            If coge_empleado_m4(datos_usuario) Or datos_usuario.permiso = 99 Then
                valida_entrada = True
            End If

        End If

        If valida_entrada Then
            lit_error = "Entrada del usuario " & Trim(datos_usuario.nombre) & " (" & Trim(datos_usuario.codemp) & ") de " & Trim(datos_usuario.codser) & "."
            If WS.inserta_log(datos_usuario.codemp, 0, lit_error) Then
                lit_error = ""
            Else
                lit_error = "Problema de seguridad. Sin acceso a log, contácte con Informática."
                valida_entrada = False
            End If
        End If

        Return valida_entrada

    End Function

    Public Shared Function vale_codigo_permiso(ByVal codigo As Integer) As Boolean
        Dim str_SQL As String

        str_SQL = "SELECT COUNT(*) " & _
                    "FROM permiso_acceso " & _
                   "WHERE cod_acceso = " & codigo & ";"

        'Valida que el codigo corresponda a uno válido
        Using Conn As New SqlClient.SqlConnection

            exporta_log(Conn)

            Conn.ConnectionString = funciones.conexion_config("cadena_conexion_telemed").ConnectionString
            Dim Devuelve As SqlClient.SqlDataReader = coge_datos_SQLSERVER(str_SQL, Conn)

            If Devuelve.HasRows Then
                Do While Devuelve.Read
                    If Devuelve.GetValue(0) > 0 Then
                        vale_codigo_permiso = True
                    End If
                Loop
            End If

            'Cerramos la conexion antes de actualizar perfiles
            Devuelve.Close()
            If Conn.State = ConnectionState.Open Then
                Conn.Close()
            End If

        End Using

        Return vale_codigo_permiso

    End Function

    Public Shared Function trocear_texto(ByVal texto As String, _
                                         ByRef texto1 As String, _
                                         ByRef texto2 As String, _
                                         ByRef texto3 As String) As Boolean
        Dim desde As Integer = 1
        'Si el texto es muy largo lo troceamos
        If Len(Trim(texto)) > 50 Then
            Dim longitud As Integer = InStr(50, texto, " ", CompareMethod.Text)
            If longitud < 1 Then longitud = Len(Trim(texto))
            texto1 = Trim(Mid(texto, desde, longitud))
            desde = desde + longitud

            If Len(Trim(texto)) > (desde + 50) Then
                longitud = InStr(desde + 50, texto, " ", CompareMethod.Text) - desde
                If longitud < 1 Then longitud = Len(Trim(texto))
                texto2 = Trim(Mid(texto, desde, longitud))

                desde = desde + longitud + 1
                longitud = Len(Trim(texto)) - desde + 1
                If longitud < 1 Then longitud = Len(Trim(texto))
                texto3 = Trim(Mid(texto, desde, longitud))
            Else
                longitud = Len(Trim(texto)) - desde + 1
                If longitud < 1 Then longitud = Len(Trim(texto))
                texto2 = Trim(Mid(texto, desde, longitud))
                texto3 = ""
            End If
        Else
            texto2 = texto
            texto1 = ""
            texto3 = ""
        End If

    End Function

    Public Shared Function datos_registro_existe(ByVal SQL As String, _
                                           ByRef datoa As String, _
                                           ByRef datob As String) As Boolean

        Dim WS As New WStelemed.telediagnosticoWS

        Try
            Using Conn As New SqlClient.SqlConnection

                Conn.ConnectionString = funciones.conexion_config("cadena_conexion_telemed").ConnectionString
                Dim Devuelve As SqlClient.SqlDataReader = coge_datos_SQLSERVER(SQL, Conn)

                'SELECT nomenf
                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        datoa = WS.mira_nulo(Devuelve.GetValue(0))
                        datob = Devuelve.GetValue(1)
                        datos_registro_existe = True
                    Loop
                End If
                Devuelve.Close()

                If Conn.State = ConnectionState.Open Then
                    Conn.Close()
                End If

            End Using

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return datos_registro_existe

    End Function

    Public Shared Function leer_parametro(ByVal tipo As String) As String
        Dim WS As New WStelemed.telediagnosticoWS
        Dim str_SQL As String
        Using Conn As New SqlClient.SqlConnection

            leer_parametro = "SIN DATO para " & tipo & " EN tabla parametros."
            Conn.ConnectionString = funciones.conexion_config("cadena_conexion_telemed").ConnectionString
            Try
                str_SQL = "SELECT valor " & _
                            "FROM parametros " & _
                           "WHERE tipo = '" & Trim(tipo) & "';"

                Dim Devuelve As SqlClient.SqlDataReader = coge_datos_SQLSERVER(str_SQL, Conn)
                'Recogido el literal, reemplaza para poner saltos de linea
                If Devuelve.HasRows Then
                    Do While Devuelve.Read
                        leer_parametro = Replace(Devuelve.GetValue(0), "(vbCrLf)", vbCrLf)
                    Loop
                End If

                Devuelve.Close()

            Catch ex As Exception
                Console.WriteLine(ex.Message)
                WS.inserta_log("ERROR", 0, "MENSAJE en leer_parametro:" & ex.Message)
            End Try

            If Conn.State = ConnectionState.Open Then
                Conn.Close()
            End If

            Return leer_parametro

        End Using

    End Function

    Public Shared Function cierro(ByRef datos_usuario As WStelemed.cls_usuario, _
                                  ByRef Session As System.Web.SessionState.HttpSessionState) As Boolean

        datos_usuario = New WStelemed.cls_usuario

        Session.Remove("datos_usuario")

        Session.Contents.RemoveAll()
        Session.Clear()
        FormsAuthentication.SignOut()

    End Function


    Public Shared Function IsSessionTimedOut(ByVal datos_usuario As WStelemed.cls_usuario) As Boolean

        Dim ctx As HttpContext = HttpContext.Current
        'Comprobamos que haya sesión en primer lugar 
        '(por ejemplo si EnableSessionState=false)
        If ctx.Session Is DBNull.Value Then Return False 'Si no hay sesión, no puede caducar

        'Se comprueba si se ha generado una nueva sesión en esta petición
        If Not ctx.Session.IsNewSession Then Return False 'Si no es una nueva sesión es que no ha caducado

        Dim objCookie As HttpCookie = ctx.Request.Cookies("ASP.NET_SessionId")
        'Esto en teoría es imposible que pase porque si hay una 
        'nueva sesión debería existir la cookie, pero lo compruebo porque
        'IsNewSession puede dar True sin ser cierto
        If objCookie Is Nothing Then Return False

        'Si hay un valor en la cookie es que hay un valor de sesión previo, pero como la sesión 
        'es nueva no debería estar, por lo que deducimos que la sesión anterior ha caducado
        If Len(objCookie.Value) > 0 Then Return True

        Return False

    End Function

End Class


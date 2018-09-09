USE [TRAtaMe]
GO
/****** Objeto:  Table [dbo].[respuesta]    Fecha de la secuencia de comandos: 08/28/2018 09:47:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[respuesta](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[idepisodio] [bigint] NOT NULL,
	[codser] [nvarchar](4) NOT NULL,
	[usuario] [nchar](10) NULL,
	[fecha] [datetime] NULL,
	[doctor] [nchar](200) NULL,
	[respuesta] [varchar](max) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Objeto:  Table [dbo].[log]    Fecha de la secuencia de comandos: 08/28/2018 09:47:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[log](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[fecha_hora] [datetime] NULL,
	[usuario] [nvarchar](50) NULL,
	[id_solicitud] [bigint] NULL,
	[observaciones] [nvarchar](max) NULL
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[fichero]    Fecha de la secuencia de comandos: 08/28/2018 09:47:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[fichero](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[id_paciente] [bigint] NULL,
	[idepisodio] [bigint] NULL,
	[fecha] [datetime] NULL,
	[tipofic] [nchar](1) NULL,
	[nomfic] [nchar](30) NULL,
	[enlace] [nchar](200) NULL
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[paciente]    Fecha de la secuencia de comandos: 08/28/2018 09:47:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[paciente](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[nhc] [nchar](10) NULL,
	[dni] [nchar](10) NULL,
	[cipa] [nchar](50) NULL,
	[nombre] [nchar](50) NULL,
	[apellido1] [nchar](100) NULL,
	[apellido2] [nchar](100) NULL,
	[fecnac] [datetime] NULL,
	[calle] [nchar](100) NULL,
	[numero] [nchar](10) NULL,
	[codpos] [nchar](5) NULL,
	[codpro] [smallint] NULL,
	[provincia] [nchar](50) NULL,
	[codmun] [smallint] NULL,
	[municipio] [nchar](50) NULL,
	[numtel1] [nchar](15) NULL,
	[numtel2] [nchar](15) NULL,
	[tipodom] [nchar](1) NULL
) ON [PRIMARY]
GO
/****** Objeto:  Table [dbo].[perfiles]    Fecha de la secuencia de comandos: 08/28/2018 09:47:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[perfiles](
	[per_codemp] [nvarchar](10) NOT NULL,
	[per_clave] [nvarchar](50) NOT NULL,
	[per_permiso] [int] NOT NULL,
	[per_dni] [nchar](10) NULL,
	[per_nombre] [nvarchar](100) NOT NULL,
	[per_codser] [nvarchar](4) NOT NULL,
	[per_email] [char](50) NULL,
	[per_origen] [nvarchar](50) NULL,
	[per_host] [nvarchar](50) NULL,
	[per_aut_user] [nvarchar](50) NULL,
	[per_aut_pass] [nvarchar](max) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Objeto:  Table [dbo].[episodio]    Fecha de la secuencia de comandos: 08/28/2018 09:47:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[episodio](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[usuario] [nchar](10) NULL,
	[fecha] [datetime] NULL,
	[doctor] [nchar](200) NULL,
	[idpaciente] [bigint] NULL,
	[pregunta] [varchar](max) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO

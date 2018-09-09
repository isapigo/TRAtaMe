// ------------------------------------------------------------------------------
// Autora: Isabel Piñuel González 
// Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
// Universidad Internacional de la Rioja (UNIR)
// Este código se ofrece bajo licencia MIT
// ------------------------------------------------------------------------------ 

Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(beginReq);
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endReq);

function beginReq(sender, args) {
	// muestra el popup 
	   $find(ModalProgress).show();
}

function endReq(sender, args) {
	// esconde el popup
	   $find(ModalProgress).hide();
	   var btn_mensaje_progress = document.getElementById("btn_mensaje_progress");
	   if (btn_mensaje_progress) {
         	btn_mensaje_progress.click();
       }
}

function ChangePaciente(sender)
       {
       var cmb_pacientes = document.getElementById("WUC_consulta_cmb_pacientes");
	   if (cmb_pacientes) {
	      var hid_indserv = document.getElementById("WUC_consulta_hid_indserv");
	      hid_indserv.value = cmb_pacientes.selectedIndex
	      cmb_pacientes.text = cmb_pacientes.options[cmb_pacientes.selectedIndex].text;
       }
}

function Changerdb(es)
       {
       var rdb_foto = document.getElementById("WUC_consulta_rdb_foto");
       var rdb_video = document.getElementById("WUC_consulta_rdb_video");
       var btn_clic_rdb = document.getElementById("WUC_consulta_btn_clic_rdb");
	   if (es=="rdb_video") {
	       rdb_foto.checked = false;
	       rdb_video.checked = true;
	       btn_clic_rdb.click();
	   }else{
	       rdb_video.checked = false;
	       rdb_foto.checked = true;
	       btn_clic_rdb.click();
	   }
}


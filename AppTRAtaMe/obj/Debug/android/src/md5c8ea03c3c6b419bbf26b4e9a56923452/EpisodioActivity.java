package md5c8ea03c3c6b419bbf26b4e9a56923452;


public class EpisodioActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onDestroy:()V:GetOnDestroyHandler\n" +
			"";
		mono.android.Runtime.register ("AppTRAtaMe.EpisodioActivity, AppTRAtaMe, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", EpisodioActivity.class, __md_methods);
	}


	public EpisodioActivity ()
	{
		super ();
		if (getClass () == EpisodioActivity.class)
			mono.android.TypeManager.Activate ("AppTRAtaMe.EpisodioActivity, AppTRAtaMe, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onDestroy ()
	{
		n_onDestroy ();
	}

	private native void n_onDestroy ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}

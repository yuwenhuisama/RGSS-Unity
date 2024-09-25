module Kernel
  def rgss_main(&callback)
    $rgss_main_callback = callback
  end

  def rgss_stop
    $rgss_stop_flag = true
  end

  def load_data(filename)
    file_full_path = File.join($rmva_project_base_path, "RMProject", filename)
    f = File.open(file_full_path, "rb")
    Marshal.load(f)
  end

  def save_data(obj, filename)
    file_full_path = File.join($rmva_project_base_path, "RMProject", filename)
    File.open(file_full_path, "wb") do |f|
      Marshal.dump(obj, f)
    end
  end

  def msgbox(*args)
    Unity.msgbox(*args)
  end

  def msgbox_p(*args)
    msgbox(*args)
  end
end

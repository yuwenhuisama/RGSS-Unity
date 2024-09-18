module Kernel
  def rgss_main(proc)
    proc.call
  end

  def rgss_stop
  end

  def load_data(filename)
    Unity.load_data(filename)
  end

  def save_data(obj, filename)
    Unity.save_data(obj, filename)
  end

  def msgbox(*args)
    Unity.msgbox(*args)
  end

  def msgbox_p(*args)
    msgbox(*args)
  end
end

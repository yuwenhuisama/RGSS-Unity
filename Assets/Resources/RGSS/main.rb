def format_exc_string(e)
  exc_str = "Exception: #{e.message} \n"
  exc_str += "Backtrace: \n"
  e.backtrace.each do |line|
    exc_str += "\t" + line + "\n"
  end

  exc_str += "\n"

  # Extracting the script name and line number from the first line of the backtrace
  if e.backtrace && !e.backtrace.empty?
    first_line = e.backtrace.first
    script_name, line_number = first_line.split(":")[0..1]
    exc_str += "\tScript name: #{script_name} \n"
    exc_str += "\tLine number: #{line_number} \n"
  end

  exc_str
end

begin
  require 'kernel'
  require 'graphics'
  require 'sprite'
  require 'bitmap'
  require 'viewport'
  require 'plane'
  require 'window'

  # msgbox("create viewport")
  # viewport = Viewport.new(200, 200, 1024, 1024)

  # msgbox("create bitmap")
  # bitmap = Bitmap.new("Book.png")
  # bitmap2 = Bitmap.new("Castle.png")
  #
  # bitmap3 = Bitmap.new(1920, 1080)
  # bitmap3.stretch_blt(Rect.new(0, 0, 1920, 1080), bitmap, Rect.new(0, 0, bitmap.width, bitmap.height), 255)
  # bitmap4 = Bitmap.new(1920, 1080)
  # bitmap4.stretch_blt(Rect.new(0, 0, 1920, 1080), bitmap2, Rect.new(0, 0, bitmap2.width, bitmap2.height), 255)
  # bitmap = Bitmap.new(300, 300)
  # bitmap.fill_rect(0, 0, 300, 300, Color::RED)

  # (0..100).each { |i|
  #   (0..100).each { |j|
  #     bitmap.set_pixel(i, j, Color::WHITE)
  #   }
  # }

  # bitmap.stretch_blt(Rect.new(50, 50, 200, 200), bitmap_img, Rect.new(20, 20, 300, 300), 255)
  # bitmap.blt(0, 0, bitmap_img, Rect.new(0, 0, 200, 200), 255)
  # bitmap.gradient_fill_rect(0, 0, 300, 300, Color::RED, Color::BLUE)

  # bitmap.blur
  # bitmap.radial_blur(5, 10)
  # bitmap.hue_change(180)
  # bitmap.draw_text(0, 0, 300, 300, "Hello World", 0)

  # msgbox("create script")
  # sprite = Sprite.new
  # sprite.bitmap = bitmap3
  # sprite.oy = 50
  # sprite.ox = 50
  # sprite.x = 0
  # sprite.y = 0
  # sprite.zoom_x = 1
  # sprite.zoom_y = 1

  # Graphics.update
  # Graphics.freeze

  # sprite.bitmap = bitmap4
  # Graphics.transition(120, "BattleStart.png")

  # sprite.opacity = 100
  # sprite.src_rect = Rect.new(0, 0, 10, 10)

  # sprite.wave_amp = 80
  # sprite.wave_length = 10
  # sprite.wave_speed = 1
  # sprite.flash(nil, 60)

  # Graphics.fadeout(60)

  # bitmap = Bitmap.new("Book.png")
  #bitmap2 = Cache.title1("Castle")
  # bitmap.radial_blur(1,1)

  # bitmap2 = Bitmap.new(100,100)
  # bitmap2.blt 0, 0, bitmap, Rect.new(0, 0, 100, 100)

  # sprite = Plane.new
  # sprite.bitmap = bitmap2
  # sprite.ox = 50
  # sprite.oy = 50
  #sprite.x = 0
  #sprite.y = 0
  # sprite.zoom_x = 2
  # sprite.zoom_y = 2

  window = Window.new(10, 10, 400, 300)
  window.windowskin = Bitmap.new("Window.png")

  # contents = Bitmap.new("Book.png")
  # contents.fill_rect(0, 0, 200, 400, Color::RED)
  window.ox = 0
  window.oy = 0
  # window.contents = contents
  window.cursor_rect = Rect.new(20, 20, 200, 200)
  window.pause = true
  
  window.openness = 0

rescue Exception => e
  str = format_exc_string(e)
  Unity.on_top_exception(str)
  return
end

Unity.on_update = proc do
  # to catch unhandled exceptions and notify native side
  begin
    # sprite.update
    # if cnt % 10 == 0
    #   msgbox("cnt", cnt)
    #   window.ox = window.ox + 1
    #   window.oy = window.oy + 1
    # end
    # cnt += 1
    if window.openness < 255
      window.openness += 1
    end
    window.update
    ::Graphics.update
  rescue Exception => e
    str = format_exc_string(e)
    Unity.on_top_exception(str)
  end
end
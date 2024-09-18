require 'type_check_util'

module Graphics
  extend TypeCheckUtil

  class << self
    DEFAULT_TYPE_CHECK_MAP = {
      frame_rate: Integer,
      frame_count: Integer,
      brightness: Integer,
    }

    DEFAULT_TYPE_CHECK_MAP.each do |prop, type|
      define_method(prop) do
        Unity::Graphics.send(prop)
      end
      define_method("#{prop}=") do |value|
        check_type(value, type)
        Unity::Graphics.send("#{prop}=", value)
      end
    end
  end

  def self.frame_rate
    Unity::Graphics.frame_rate
  end

  def self.frame_rate=(value)
    check_type(value, Integer)
    Unity::Graphics.frame_rate = value
  end

  def self.frame_count
    Unity::Graphics.frame_count
  end

  def self.frame_count=(value)
    check_type(value, Integer)
    Unity::Graphics.frame_count = value
  end

  def self.brightness
    Unity::Graphics.brightness
  end

  def self.brightness=(value)
    check_type(value, Integer)
    Unity::Graphics.brightness = value
  end

  def self.wait(duration)
    check_arguments([duration], [Integer])
    Unity::Graphics.wait(duration)
  end

  def self.fadeout(duration)
    check_arguments([duration], [Integer])
    Unity::Graphics.fadeout(duration)
  end

  def self.fadein(duration)
    check_arguments([duration], [Integer])
    Unity::Graphics.fadein(duration)
  end

  def self.play_movie(filename)
    check_arguments([filename], [String])
    Unity::Graphics.play_movie(filename)
  end

  def self.transition(duration = 10, filename = "", vague = 40)
    check_arguments([duration, filename, vague], [Integer, String, Integer])
    Unity::Graphics.transition(duration, filename, vague)
  end

  def self.resize_screen(width = 1920, height = 1080)
    check_arguments([width, height], [Integer, Integer])
    Unity::Graphics.resize_screen(width, height)
  end

  [:update, :freeze, :snap_to_bitmap, :frame_reset, :width, :height].each do |method_name|
    define_singleton_method(method_name) do |*args|
      Unity::Graphics.send(method_name, *args)
    end
  end
end
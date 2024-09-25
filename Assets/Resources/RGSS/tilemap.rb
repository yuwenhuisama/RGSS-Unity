require 'type_check_util'

class Tilemap
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(viewport = nil)
    @__handler__ = Unity::Tilemap.new(viewport)
  end

  [:dispose, :disposed?, :update].each do |method|
    define_method(method) { @__handler__.send(method) }
  end

  def bitmaps
    @__handler__.bitmaps.map { |bitmap| Unity::Bitmap.new(bitmap) }
  end

  def map_data
    @__handler__.map_data
  end

  def map_data=(map_data)
    check_arguments([map_data], [Table])
    @__handler__.map_data = map_data.__handler__
  end

  def flash_data
    @__handler__.flash_data
  end

  def flash_data=(flash_data)
    check_arguments([flash_data], [Table])
    @__handler__.flash_data = flash_data.__handler__
  end

  def flags
     @__handler__.flags
  end

  def flags=(flags)
    check_arguments([flags], [Table])
    @__handler__.flags = flags.__handler__
  end

  def viewport
    @__handler__.viewport
  end

  def viewport=(viewport)
    check_arguments([viewport], [Viewport])
    @__handler__.viewport = viewport.__handler__
  end

  def eql?(other)
    if self == other
      true
    end
    self.__handler__ == other.__handler__
  end

  def hash
    @__handler__.hash
  end

  [:ox, :oy, :visible].each do |prop|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") { |value| @__handler__.send("#{prop}=", value) }
  end
end
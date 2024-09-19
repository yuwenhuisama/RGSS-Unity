require 'type_check_util'
require 'table'
require 'viewport'

class Tilemap
  include TypeCheckUtil

  attr_reader :handler

  def initialize(viewport = nil)
    @handler = Unity::Tilemap.new(viewport)
  end

  [:dispose, :disposed?, :update].each do |method|
    define_method(method) { @handler.send(method) }
  end

  def bitmaps
    @handler.bitmaps.map { |bitmap| Unity::Bitmap.new(bitmap) }
  end

  def map_data
    @handler.map_data
  end

  def map_data=(map_data)
    check_arguments([map_data], [Table])
    @handler.map_data = map_data.handler
  end

  def flash_data
    @handler.flash_data
  end

  def flash_data=(flash_data)
    check_arguments([flash_data], [Table])
    @handler.flash_data = flash_data.handler
  end

  def flags
     @handler.flags
  end

  def flags=(flags)
    check_arguments([flags], [Table])
    @handler.flags = flags.handler
  end

  def viewport
    @handler.viewport
  end

  def viewport=(viewport)
    check_arguments([viewport], [Viewport])
    @handler.viewport = viewport.handler
  end

  def eql?(other)
    if self == other
      true
    end
    self.handler == other.handler
  end

  def hash
    @handler.hash
  end

  [:ox, :oy, :visible].each do |prop|
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") { |value| @handler.send("#{prop}=", value) }
  end
end
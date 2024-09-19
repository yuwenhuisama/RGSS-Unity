require 'color'
require 'type_check_util'

class Font
  include TypeCheckUtil

  attr_reader :handler

  class << self
    DEFAULT_TYPE_CHECK_MAP = {
      default_size: Integer,
      default_bold: [TrueClass, FalseClass],
      default_italic: [TrueClass, FalseClass],
      default_shadow: [TrueClass, FalseClass],
      default_outline: [TrueClass, FalseClass],
      default_color: Color,
      default_out_color: Color
    }

    def default_name
      @default_name
    end

    def default_name=(value)
      check_type(value, [String, Array])

      if value.is_a? Array
        value.each { |v| check_type(v, String) }
        @default_name = value
      else
        @default_name = [value]
      end

    end

    DEFAULT_TYPE_CHECK_MAP.each do |prop, type|
      define_method(prop) { instance_variable_get("@#{prop}") }
      define_method("#{prop}=") do |val|
        check_type(val, type)
        instance_variable_set("@#{prop}", val)
      end
    end
  end

  @default_name = ["Arial"]
  @default_size = 24
  @default_bold = false
  @default_italic = false
  @default_shadow = false
  @default_outline = true
  @default_color = Color::WHITE
  @default_out_color = Color.new(0, 0, 0, 128)

  TYPE_CHECK_MAP = {
    size: Integer,
    bold: [TrueClass, FalseClass],
    italic: [TrueClass, FalseClass],
    shadow: [TrueClass, FalseClass],
    outline: [TrueClass, FalseClass],
  }

  def initialize(name, size = Font.default_size)
    if name.is_a? String
      @handler = Unity::Font.new_ns([name], size)
    elsif name.is_a? Array
      name.each { |v| check_type(v, String) }
      @handler = Unity::Font.new_ns(name, size)
    else
      raise TypeError, "Invalid argument type"
    end

    @handler.bold = Font.default_bold
    @handler.italic = Font.default_italic
    @handler.shadow = Font.default_shadow
    @handler.outline = Font.default_outline
    @handler.color = Font.default_color.handler
    @handler.out_color = Font.default_out_color.handler
  end

  def color
    Color.new @handler.color
  end

  def color=(value)
    check_type(value, Color)
    @handler.color = value.handler
  end

  def out_color
    Color.new @handler.out_color
  end

  def out_color=(value)
    check_type(value, Color)
    @handler.out_color = value.handler
  end

  def name
    @name
  end

  def name=(value)
    check_type(value, [String, Array])

    if value.is_a? Array
      value.each { |v| check_type(v, String) }
      @name = value
    else
      @name = [value]
    end
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

  TYPE_CHECK_MAP.each do |prop, type|
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") do |val|
      check_type(val, type)
      @handler.send("#{prop}=", val)
    end
  end

  DEFAULT_FONT = Font.new("Arial", 24)
end
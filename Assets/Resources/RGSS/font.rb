require 'type_check_util'

class Font
  include TypeCheckUtil

  attr_reader :__handler__

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

  def initialize(*args)
    if args.size == 0
      name = [Font.default_name]
      size = Font.default_size
      @__handler__ = Unity::Font.new_ns(name, size)
    elsif args.length == 1
      arg, = args
      if arg.is_a? Unity::Font
        @__handler__ = arg
      elsif arg is_a? String
        name = [arg]
        size = Font.default_size
        @__handler__ = Unity::Font.new_ns(name, size)
      elsif arg is_a? Array
        name.each { |v| check_type(v, String) }
        name = arg
        size = Font.default_size
        @__handler__ = Unity::Font.new_ns(name, size)
      end
    elsif args.length == 2
      name, size = args
      if name.is_a? Array
        name.each { |v| check_type(v, String) }
      else
        check_type name, String
        name = [name]
      end
      check_type size, Integer
      @__handler__ = Unity::Font.new_ns(name, size)
    else
      raise TypeError, "Invalid argument type"
    end

    @__handler__.bold = Font.default_bold
    @__handler__.italic = Font.default_italic
    @__handler__.shadow = Font.default_shadow
    @__handler__.outline = Font.default_outline
    @__handler__.color = Font.default_color.__handler__
    @__handler__.out_color = Font.default_out_color.__handler__
  end

  def color
    Color.new @__handler__.color
  end

  def color=(value)
    check_type(value, Color)
    @__handler__.color = value.__handler__
  end

  def out_color
    Color.new @__handler__.out_color
  end

  def out_color=(value)
    check_type(value, Color)
    @__handler__.out_color = value.__handler__
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
    unless other.is_a? Font
      return false
    end

    if self == other || self.__handler__ == other.__handler__
      return true
    end

    self.name == other.name && self.size == other.size &&
      self.bold == other.bold && self.italic == other.italic &&
      self.shadow == other.shadow && self.outline == other.outline &&
      self.color == other.color && self.out_color == other.out_color
  end

  def hash
    @__handler__.hash
  end

  TYPE_CHECK_MAP.each do |prop, type|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") do |val|
      check_type(val, type)
      @__handler__.send("#{prop}=", val)
    end
  end

  DEFAULT_FONT = Font.new("Arial", 24)
end
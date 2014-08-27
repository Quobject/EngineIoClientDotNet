// JavaScript equivalent to printf/string.format
// http://stackoverflow.com/questions/610406/javascript-equivalent-to-printf-string-format
// First, checks if it isn't implemented yet.
if (!String.prototype.format) {
  String.prototype.format = function () {
    var args = arguments;
    return this.replace(/{(\d+)}/g, function (match, number) {
      return typeof args[number] !== 'undefined' ? args[number] : match
      ;
    });
  };
}

module.exports = function (grunt) {
  var
    node_os = require('os'),
    os = node_os.platform() === 'win32' ? 'win' : 'linux';


  grunt.log.writeln( 'os = "%s"', os );

  grunt.loadTasks('./tasks');

  grunt.initConfig({      
    os: os,
    server_path: '../EngineIoClientDotNet_Tests/Resources',
    shell: {
      exec: {
        options: {
          stdout: true,
          stderr: true
        }
      }
    },
    jshint: {
      options: {
        jshintrc: true,
      },
      target: ['Gruntfile.js', '../EngineIoClientDotNet_Tests/Resources/server.js']
    }
  });

  
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.registerTask('default', ['jshint', 'installNpm']);
};


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
